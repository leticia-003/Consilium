/**
 * Utility helpers for formatting and parsing addresses used by the frontend.
 *
 * Format convention used by create-client:
 *   parts joined by "; " and ZIP appended after an em-dash: " — "
 * Example: "Street 1; City; Country — 12345"
 */

export interface AddressParts {
  street?: string;
  cityState?: string;
  country?: string;
  zip?: string;
}

export function formatAddress(parts: AddressParts): string {
  const list: string[] = [];
  if (parts.street) list.push(parts.street.trim());
  if (parts.cityState) list.push(parts.cityState.trim());
  if (parts.country) list.push(parts.country.trim());

  let main = list.join('; ');
  if (parts.zip && parts.zip.toString().trim()) {
    const z = parts.zip.toString().trim();
    main = main ? `${main} — ${z}` : z;
  }

  return main;
}

export function parseAddress(raw: string): AddressParts {
  const out: AddressParts = {};
  const s = (raw || '').toString().trim();
  if (!s) return out;

  const zipSplit = s.split(/\s*—\s*/);
  const main = zipSplit[0] || '';
  const zip = zipSplit[1] ? zipSplit[1].trim() : '';

  const parts = main ? main.split(/\s*;\s*/).map(p => p.trim()).filter(p => p) : [];

  out.street = parts[0] || '';
  out.cityState = parts[1] || '';
  out.country = parts[2] || '';
  out.zip = zip || '';

  return out;
}
