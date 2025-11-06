export interface Country {
  code: string;
  dialCode: number;
  name: string;
  flagEmoji?: string;
}

export const COUNTRIES: Country[] = [
  { code: 'PT', dialCode: 351, name: 'Portugal', flagEmoji: '🇵🇹' },
  { code: 'US', dialCode: 1, name: 'United States', flagEmoji: '🇺🇸' },
  { code: 'CA', dialCode: 1, name: 'Canada', flagEmoji: '🇨🇦' },
  { code: 'GB', dialCode: 44, name: 'United Kingdom', flagEmoji: '🇬🇧' },
  { code: 'FR', dialCode: 33, name: 'France', flagEmoji: '🇫🇷' },
  { code: 'DE', dialCode: 49, name: 'Germany', flagEmoji: '🇩🇪' },
  { code: 'ES', dialCode: 34, name: 'Spain', flagEmoji: '🇪🇸' },
  { code: 'IT', dialCode: 39, name: 'Italy', flagEmoji: '🇮🇹' },
  { code: 'NL', dialCode: 31, name: 'Netherlands', flagEmoji: '🇳🇱' },
  { code: 'BE', dialCode: 32, name: 'Belgium', flagEmoji: '🇧🇪' },
  { code: 'LU', dialCode: 352, name: 'Luxembourg', flagEmoji: '🇱🇺' },
  { code: 'CH', dialCode: 41, name: 'Switzerland', flagEmoji: '🇨🇭' },
  { code: 'AT', dialCode: 43, name: 'Austria', flagEmoji: '🇦🇹' },
  { code: 'IE', dialCode: 353, name: 'Ireland', flagEmoji: '🇮🇪' },
  { code: 'SE', dialCode: 46, name: 'Sweden', flagEmoji: '🇸🇪' },
  { code: 'NO', dialCode: 47, name: 'Norway', flagEmoji: '🇳🇴' },
  { code: 'DK', dialCode: 45, name: 'Denmark', flagEmoji: '🇩🇰' },
  { code: 'FI', dialCode: 358, name: 'Finland', flagEmoji: '🇫🇮' },
  { code: 'PL', dialCode: 48, name: 'Poland', flagEmoji: '🇵🇱' },
  { code: 'CZ', dialCode: 420, name: 'Czech Republic', flagEmoji: '🇨🇿' },
  { code: 'SK', dialCode: 421, name: 'Slovakia', flagEmoji: '🇸🇰' },
  { code: 'HU', dialCode: 36, name: 'Hungary', flagEmoji: '🇭🇺' },
  { code: 'RO', dialCode: 40, name: 'Romania', flagEmoji: '🇷🇴' },
  { code: 'BG', dialCode: 359, name: 'Bulgaria', flagEmoji: '🇧🇬' },
  { code: 'GR', dialCode: 30, name: 'Greece', flagEmoji: '🇬🇷' },
  { code: 'TR', dialCode: 90, name: 'Turkey', flagEmoji: '🇹🇷' },
  { code: 'RU', dialCode: 7, name: 'Russia', flagEmoji: '🇷🇺' },
  { code: 'CN', dialCode: 86, name: 'China', flagEmoji: '🇨🇳' },
  { code: 'JP', dialCode: 81, name: 'Japan', flagEmoji: '🇯🇵' },
  { code: 'KR', dialCode: 82, name: 'South Korea', flagEmoji: '🇰🇷' },
  { code: 'IN', dialCode: 91, name: 'India', flagEmoji: '🇮🇳' },
  { code: 'PK', dialCode: 92, name: 'Pakistan', flagEmoji: '🇵🇰' },
  { code: 'BD', dialCode: 880, name: 'Bangladesh', flagEmoji: '🇧🇩' },
  { code: 'SG', dialCode: 65, name: 'Singapore', flagEmoji: '🇸🇬' },
  { code: 'MY', dialCode: 60, name: 'Malaysia', flagEmoji: '🇲🇾' },
  { code: 'ID', dialCode: 62, name: 'Indonesia', flagEmoji: '🇮🇩' },
  { code: 'AU', dialCode: 61, name: 'Australia', flagEmoji: '🇦🇺' },
  { code: 'NZ', dialCode: 64, name: 'New Zealand', flagEmoji: '🇳🇿' },
  { code: 'BR', dialCode: 55, name: 'Brazil', flagEmoji: '🇧🇷' },
  { code: 'AR', dialCode: 54, name: 'Argentina', flagEmoji: '🇦🇷' },
  { code: 'CL', dialCode: 56, name: 'Chile', flagEmoji: '🇨🇱' },
  { code: 'MX', dialCode: 52, name: 'Mexico', flagEmoji: '🇲🇽' },
  { code: 'ZA', dialCode: 27, name: 'South Africa', flagEmoji: '🇿🇦' },
  { code: 'NG', dialCode: 234, name: 'Nigeria', flagEmoji: '🇳🇬' },
  { code: 'EG', dialCode: 20, name: 'Egypt', flagEmoji: '🇪🇬' },
  { code: 'KE', dialCode: 254, name: 'Kenya', flagEmoji: '🇰🇪' },
  { code: 'IL', dialCode: 972, name: 'Israel', flagEmoji: '🇮🇱' },
  { code: 'SA', dialCode: 966, name: 'Saudi Arabia', flagEmoji: '🇸🇦' },
  { code: 'AE', dialCode: 971, name: 'United Arab Emirates', flagEmoji: '🇦🇪' }
];

export function findCountryByDialCode(dialCode?: number | string | null): Country | null {
  if (!dialCode && dialCode !== 0) return null;
  const code = Number(dialCode);
  return COUNTRIES.find(c => c.dialCode === code) || null;
}

export function getFlagEmoji(dialCode?: number | string | null): string {
  const c = findCountryByDialCode(dialCode);
  return c ? c.flagEmoji || '' : '';
}

export function getDialPrefix(dialCode?: number | string | null): string {
  if (!dialCode && dialCode !== 0) return '';
  return `+${Number(dialCode)}`;
}
