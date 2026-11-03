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

import { createHash, createHmac, randomBytes } from "crypto"
import { readFileSync } from "fs"
import path from "path"
import { describe, expect, it, vi } from "vitest"

vi.mock("../../components/loggingInterop", () => ({
    log: vi.fn(),
    LogLevel: { WARN: "warn", DEBUG: "debug" },
}))

import { DEV_PRODUCTS, verifyDevFile } from "../../components/devEntitlements"

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

    describe("signed file verification", () => {
        const key = randomBytes(32)
        const verifier = createHash("sha256").update(key).digest("hex")
        const now = 1_000_000

        function build(
            products: string[],
            expires = now + 3600,
            signingKey = key,
        ): string {
            const sig = createHmac("sha256", signingKey)
                .update(`${expires}\n${products.join("\n")}`)
                .digest("hex")

            return [
                `#auth:${key.toString("hex")}`,
                `#expires:${expires}`,
                `#sig:${sig}`,
                ...products,
            ].join("\r\n")
        }

        it("accepts a correctly signed, unexpired file", () => {
            expect(
                verifyDevFile(build(["SIN_GREED", "TRINITY"]), verifier, now),
            ).toEqual({ ok: true, products: ["SIN_GREED", "TRINITY"] })
        })

        it("rejects an unsigned file (the old plain format)", () => {
            const result = verifyDevFile("SIN_GREED\nTRINITY\n", verifier, now)

            expect(result.ok).toBe(false)
        })

        it("rejects a file whose product list was edited", () => {
            const edited = build(["SIN_GREED"]) + "\r\nWOA_DELUXE"

            expect(verifyDevFile(edited, verifier, now).ok).toBe(false)
        })

        it("rejects an expired file", () => {
            const result = verifyDevFile(
                build(["TRINITY"], now - 1),
                verifier,
                now,
            )

            expect(result).toEqual({ ok: false, reason: "file has expired" })
        })

        it("rejects a file made with the wrong key", () => {
            const other = randomBytes(32)
            const forged = build(["TRINITY"], now + 3600, other).replace(
                key.toString("hex"),
                other.toString("hex"),
            )

            expect(verifyDevFile(forged, verifier, now)).toEqual({
                ok: false,
                reason: "wrong key",
            })
        })

        it("rejects extended expiry on an otherwise valid file", () => {
            const valid = build(["TRINITY"], now + 10)
            const tampered = valid.replace(
                `#expires:${now + 10}`,
                `#expires:${now + 99999999}`,
            )

            expect(verifyDevFile(tampered, verifier, now).ok).toBe(false)
        })

        it("rejects malformed headers", () => {
            expect(
                verifyDevFile("#auth:zz\n#expires:1\n#sig:zz\nX", verifier, now)
                    .ok,
            ).toBe(false)
        })
    })
})
