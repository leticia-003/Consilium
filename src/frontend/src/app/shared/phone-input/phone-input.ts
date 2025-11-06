import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Country, COUNTRIES } from '../phone-countries/phone-countries';

@Component({
  selector: 'app-phone-input',
  standalone: true,
  templateUrl: './phone-input.html',
  styleUrls: ['./phone-input.css'],
  imports: [CommonModule, FormsModule]
})
export class PhoneInputComponent {
  @Input() country: Country | null = null;
  @Input() countryDialCode?: number;
  @Input() number = '';
  @Output() numberChange = new EventEmitter<string>();
  @Output() countryChange = new EventEmitter<number>(); // dial code
  @Output() valueChange = new EventEmitter<{dialCode:number, number:string, e164?:string}>();

  countries: Country[] = COUNTRIES;

  dropdownOpen = false;
  search = '';

  openDropdown() { this.dropdownOpen = true; }
  closeDropdown() { this.dropdownOpen = false; }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['countryDialCode'] && this.countryDialCode) {
      const found = COUNTRIES.find(c => c.dialCode === this.countryDialCode);
      if (found) this.country = found;
    }
  }

  selectCountry(c: Country) {
    this.country = c;
    this.countryChange.emit(c.dialCode);
    this.emitValue();
    this.closeDropdown();
  }

  onNumberInput(v: string) {
    this.number = v;
    this.numberChange.emit(v);
    this.emitValue();
  }

  emitValue() {
    const dial = this.country ? this.country.dialCode : 351;
    const e164 = `+${dial}${this.number.replace(/\D/g,'')}`;
    this.valueChange.emit({ dialCode: dial, number: this.number, e164 });
  }

  filteredCountries() {
    const q = (this.search || '').toLowerCase();
    return COUNTRIES.filter(c => c.name.toLowerCase().includes(q) || String(c.dialCode).includes(q));
  }
}
