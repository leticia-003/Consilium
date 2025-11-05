import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ChangeDetectorRef } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ButtonComponent } from '../../shared/button/button';
import { ClientService } from '../../services/client.service';
import { NotificationService } from '../../shared/notification/notification.service';
import { parseAddress, formatAddress } from '../../shared/address.util';
import { sanitizeModel, sanitizeString } from '../../shared/input.util';
import { Client } from '../../models/client';

@Component({
  selector: 'app-edit-client',
  standalone: true,
  templateUrl: './edit-client.html',
  styleUrls: ['./edit-client.css', '../create-client/create-client.css'],
  imports: [CommonModule, FormsModule, PageTitleComponent, ButtonComponent]
})
export class EditClientComponent implements OnInit {
  // toggle applies to model immediately; save persists via Save button
  id: string | null = null;
  model: Partial<
    Client & {
      password?: string;
      confirmPassword?: string;
      addressStreet?: string;
      addressCityState?: string;
      addressCountry?: string;
      addressZip?: string;
    }
  > = {
    name: '',
    email: '',
    nif: '',
    phone: '',
    addressStreet: '',
    addressCityState: '',
    addressCountry: '',
    addressZip: '',
    isActive: true,
    password: '',
    confirmPassword: ''
  };

  submitting = false;
  fieldErrors: Record<string, string[]> = {};
  generalError = '';

  constructor(
    private route: ActivatedRoute,
    private clientService: ClientService,
    private notifications: NotificationService,
    public router: Router,
    private cd: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    if (!this.id) {
      this.notifications.showError('No client id provided.');
      return;
    }

    this.loadClient(this.id);
  }

  onActiveToggle(checked: boolean) {
    // apply toggle to model; actual persist happens on Save()
    this.model.isActive = !!checked;
  }

  loadClient(id: string) {
    this.clientService.getClient(id).subscribe({
      next: (data: any) => {
        // populate model fields
        this.model.name = data?.name || '';
        this.model.email = data?.email || '';
        this.model.nif = data?.nif || '';
        this.model.phone = data?.phone || '';
        this.model.isActive = (data?.status || '').toString().toUpperCase() === 'ACTIVE';

        // parse address into parts
        const parsed = parseAddress(data?.address || '');
        this.model.addressStreet = parsed.street || '';
        this.model.addressCityState = parsed.cityState || '';
        this.model.addressCountry = parsed.country || '';
        this.model.addressZip = parsed.zip || '';
        // ensure view reflects async-loaded model immediately
        try { this.cd.detectChanges(); } catch (e) {}
      },
      error: (err) => {
        console.error('Failed to load client', err);
        this.notifications.showError('Failed to load client.');
      }
    });
  }

  get initials(): string {
    const name = (this.model.name || '').trim();
    if (!name) return '👤';
    const parts = name.split(/\s+/).filter(p => p.length > 0);
    const initials = parts.map(p => p.charAt(0)).join('').slice(0, 2).toUpperCase();
    return initials || '👤';
  }

  isPasswordValid(): boolean {
    const p = this.model.password || '';
    const c = this.model.confirmPassword || '';
    return p.length === 0 ? true : (p.length >= 8 && p === c);
  }

  isComplete(): boolean {
    const s = (v?: string) => (v || '').toString().trim().length > 0;
    const email = (this.model.email || '').toString().trim();
    const emailOk = /\S+@\S+\.\S+/.test(email);

    return (
      s(this.model.name) &&
      emailOk &&
      s(this.model.nif) &&
      s(this.model.phone) &&
      s(this.model.addressStreet) &&
      s(this.model.addressCityState) &&
      s(this.model.addressCountry) &&
      s(this.model.addressZip) &&
      this.isPasswordValid()
    );
  }

  save() {
    if (!this.id || this.submitting) return;
    if (!this.isPasswordValid()) {
      this.notifications.showError('Password is required and must match confirmation.');
      return;
    }

    this.submitting = true;
    this.fieldErrors = {};
    this.generalError = '';

    const sanitized = sanitizeModel(this.model as Record<string, any>);
    const payload: any = { ...sanitized };
    payload.address = formatAddress({
      street: sanitizeString(this.model.addressStreet || ''),
      cityState: sanitizeString(this.model.addressCityState || ''),
      country: sanitizeString(this.model.addressCountry || ''),
      zip: sanitizeString(this.model.addressZip || ''),
    });

    // remove transient fields
    delete payload.addressStreet;
    delete payload.addressCityState;
    delete payload.addressCountry;
    delete payload.addressZip;
    delete payload.confirmPassword;

    this.clientService.updateClient(this.id, payload).subscribe({
      next: _ => {
        this.notifications.showSuccess('Client updated successfully');
        setTimeout(() => this.router.navigate(['/clients']), 600);
      },
      error: (err) => {
        console.error('Update client failed', err);
        const body = err?.error || {};
        if (body && typeof body === 'object' && body.errors) {
          for (const k of Object.keys(body.errors)) {
            const key = k.toString().toLowerCase();
            const vals = Array.isArray(body.errors[k]) ? body.errors[k].map((v: any) => String(v)) : [String(body.errors[k])];
            this.fieldErrors[key] = vals;
          }
          this.notifications.showError('Please correct the highlighted fields.');
        } else if (body && (body.message || body.error)) {
          this.generalError = (body.message || body.error).toString();
          this.notifications.showError(this.generalError || 'Failed to update client.');
        } else {
          this.notifications.showError('Failed to update client.');
        }
        this.submitting = false;
      }
    });
  }
}
