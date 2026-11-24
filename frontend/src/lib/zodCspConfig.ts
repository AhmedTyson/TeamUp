/**
 * @date 2026-11-24
 * @purpose Disables Zod v4 JIT (new Function) compilation so form schemas run cleanly under strict script-src 'self' CSP without 'unsafe-eval'.
 */
import { z } from "zod";

z.config({ jitless: true });

export { z };
