/*
 *     The Peacock Project - a HITMAN server replacement.
 *     Copyright (C) 2021-2026 The Peacock Project Team
 *
 *     This program is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU Affero General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *
 *     This program is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU Affero General Public License for more details.
 *
 *     You should have received a copy of the GNU Affero General Public License
 *     along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

import { existsSync, readFileSync } from "fs"
import { resolve } from "path"
import { log, LogLevel } from "./loggingInterop"

/**
 * DEV TESTING ONLY.
 *
 * When a file named devEntitlements.txt exists next to options.ini, the entitlements the
 * server believes the user owns are REPLACED by the products listed in that file
 * (one product key per line). This lets you test any combination of owned products
 * against the real ownership logic in inventory.ts. Delete the file to go back to normal.
 *
 * The IDs below are the ones inventory.ts checks (Epic hex id + Steam app id of each product).
 */
export const DEV_ENTITLEMENTS_FILE = "devEntitlements.txt"

export const DEV_PRODUCTS: Record<string, string[]> = {
    // H3 Access Pass: H2 Expansion
    H2_EXPANSION_PASS: ["afa4b921503f43339c360d4b53910791", "1829590"],
    // WoA Deluxe (grants many packs)
    WOA_DELUXE: ["84a1a6fda4fb48afbb78ee9b2addd475"],
    // H2 Executive pack
    H2_EXECUTIVE: ["6408de14f7dc46b9a33adcf6cbc4d159"],
    // WoA Standard (H1 items)
    WOA_STANDARD: ["a3509775467d4d6a8a7adffe518dc204", "1843460"],
    // H1 Requiem pack
    H1_REQUIEM: ["e698e1a4b63947b0bc9349a5ae2dc015"],
    // H1 GOTY pack
    H1_GOTY: ["894d1e6771044f48a8fdde934b8e443a", "1829595"],
    // H2 Winter Sports (Steam)
    H2_WINTERSPORTS_STEAM: ["957693"],
    // H3 Deluxe pack
    DELUXE_PACK: ["bc610b36c75442299edcbe99f6f0fb60", "1829591"],
    // Sin: Greed
    SIN_GREED: ["0e8632b4cdfb415e94291d97d727b98d", "1829580"],
    // Sin: Pride
    SIN_PRIDE: ["3f9adc216dde44dda5e829f11740a0a2", "1829581"],
    // Sin: Sloth
    SIN_SLOTH: ["aece009ff59441c0b526f8aa69e24cfb", "1829582"],
    // Sin: Lust
    SIN_LUST: ["dfe5aeb89976450ba1e0e2c208b63d33", "1829583"],
    // Sin: Gluttony
    SIN_GLUTTONY: ["30107bff80024d1ab291f9cd3bac9fac", "1829584"],
    // Sin: Envy
    SIN_ENVY: ["0403062df0d347619c8dcf043c65c02e", "1829585"],
    // Sin: Wrath
    SIN_WRATH: ["9e936ed2507a473db6f53ad24d2da587", "1829586"],
    // Trinity pack
    TRINITY: ["5d06a6c6af9b4875b3530d5328f61287", "1829596"],
    // Makeshift pack
    MAKESHIFT: ["08d2bc4d20754191b6c488541d2b4fa1", "2184791"],
    // Concrete Art pack
    CONCRETEART: ["a1e9a63fa4f3425aa66b9b8fa3c9cc35", "2184790"],
    // Sambuca pack (H3 Sambuca only - Penicillin and Vanity Sport are separate products)
    SAMBUCA: ["9220c020262f420da06eb46a4b1ce86f", "2828470"],
    // H3 Elusive Target: Penicillin (also unlocks the Penicillin items)
    PENICILLIN: ["6cdf07da030d4f66acd50eaf3cd234c7", "2973650"],
    // H3 Vanity Sport pack (also unlocks the Penicillin items)
    VANITY_SPORT: ["16bcef4f91674b00ba3d7f2d4f629cec", "4542910"],
    // Tomorrowland pack
    TOMORROWLAND: ["f04198e0ffcf49079b5ec77bb6b66891", "3110360"],
    // Lambic pack
    LAMBIC: ["70a9afcc8de84b6ab0f2b45b2018559b", "3254350"],
    // French Martini pack
    FRENCHMARTINI: ["256eeeb3d8044aa1840e1606d268e0b2", "3711140"],
    // Baiju pack
    BAIJU: ["04cb1b3e5b424308be25236f6bc1b2fb", "3957470"],
    // Bellini pack
    BELLINI: ["0047ddcd5e6846e881f1037c1416e3d9", "4097630"],
    // Filur pack
    FILUR: ["b135c766d25948c39d7dd316dbc4db53", "4328240"],
    // Pomada pack
    POMADA: ["d51a3a65928841d5b4cabad20a865006", "4621250"],
    // EG04 pack
    EG04: ["7b3bf47c436644ea8fea4f95317d431c", "4944070"],
    // Greentini pack
    GREENTINI: ["d396d245e23a401b8422116db9026a27", "4911210"],
    // EG05 pack
    EG05: ["34cd5f9f19ab44cc8766089dab207bf0", "5235600"],
    // Shamrock pack
    SHAMROCK: ["4bbcb2ba387642e694d3c9c9b74b4919", "5235590"],
}

let lastAnnounced: string | undefined

/**
 * Logs (at info level, so it is visible by default) only when the dev state changes,
 * so the console is not spammed on every inventory build.
 */
function announce(signature: string, message: string): void {
    if (signature === lastAnnounced) {
        return
    }

    lastAnnounced = signature
    log(LogLevel.INFO, message, "DevEntitlements")
}

/**
 * Returns the entitlements to use for ownership checks.
 * Must be used everywhere the server reads a user's owned products (inventory,
 * the entitlements sent to the game, web features), never persisted.
 *
 * @param real The user's real entitlements.
 */
export function applyDevEntitlements(real: string[]): string[] {
    if (!existsSync(DEV_ENTITLEMENTS_FILE)) {
        announce(
            "off",
            `${resolve(DEV_ENTITLEMENTS_FILE)} not found - using real entitlements.`,
        )
        return real
    }

    const selected = readFileSync(DEV_ENTITLEMENTS_FILE)
        .toString()
        .split(/\r?\n/)
        .map((line) => line.trim())
        .filter((line) => line.length > 0)

    const ids = new Set<string>()

    for (const key of selected) {
        const productIds = DEV_PRODUCTS[key]

        if (!productIds) {
            log(
                LogLevel.WARN,
                `Unknown product "${key}" in ${DEV_ENTITLEMENTS_FILE} - ignoring it.`,
                "DevEntitlements",
            )
            continue
        }

        for (const id of productIds) {
            ids.add(id)
        }
    }

    announce(
        selected.join(","),
        `Dev entitlements ACTIVE from ${resolve(DEV_ENTITLEMENTS_FILE)}: ${selected.length} product(s) selected (${selected.join(", ") || "none"}). Real entitlements are ignored.`,
    )

    return [...ids]
}
