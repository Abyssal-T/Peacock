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

import { readFileSync } from "fs"
import path from "path"
import { describe, expect, it, vi } from "vitest"

vi.mock("../../components/loggingInterop", () => ({
    log: vi.fn(),
    LogLevel: { WARN: "warn", DEBUG: "debug" },
}))

import { DEV_PRODUCTS } from "../../components/devEntitlements"

describe("dev entitlements", () => {
    it("keeps the Dev Panel (C#) and server (TS) product lists in sync", () => {
        const source = readFileSync(
            path.join(
                __dirname,
                "../../patcher/HitmanPatcher.Core/DevProducts.cs",
            ),
            "utf-8",
        )

        const csKeys = [...source.matchAll(/new\[\] \{ "([A-Z0-9_]+)",/g)].map(
            (m) => m[1],
        )

        expect(csKeys.length).toBeGreaterThan(0)
        expect([...csKeys].sort()).toEqual(Object.keys(DEV_PRODUCTS).sort())
    })

    it("does not share product IDs between different products", () => {
        const seen = new Map<string, string>()

        for (const [key, ids] of Object.entries(DEV_PRODUCTS)) {
            for (const id of ids) {
                expect(
                    seen.get(id),
                    `${id} is in ${key} and ${seen.get(id)}`,
                ).toBeUndefined()
                seen.set(id, key)
            }
        }
    })
})
