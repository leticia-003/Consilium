/**
 * Minimal client-side input sanitization helpers.
 * NOTE: This is only a convenience normalization layer. Server-side
 * validation and sanitization are still required and authoritative.
 */

export function sanitizeString(value: any, maxLen = 2000): string {
  if (value == null) return '';
  let s = String(value);
  // remove control characters except common whitespace (tab/newline)
  s = s.replace(/[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]+/g, '');
  // normalize whitespace: collapse multiple spaces/newlines into single space
  s = s.replace(/[\s\u00A0]+/g, ' ').trim();
  if (s.length > maxLen) s = s.slice(0, maxLen);
  return s;
}

export function sanitizeModel(obj: Record<string, any>): Record<string, any> {
  const out: Record<string, any> = {};
  for (const k of Object.keys(obj)) {
    const v = obj[k];
    if (v == null) continue;
    if (typeof v === 'string') {
      out[k] = sanitizeString(v);
    } else if (typeof v === 'number' || typeof v === 'boolean') {
      out[k] = v;
    } else {
      // fallback: stringify and sanitize
      out[k] = sanitizeString(String(v));
    }
  }
  return out;
}
