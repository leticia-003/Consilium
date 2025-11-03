import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-button',
  standalone: true,
  template: `
    <a
      class="btn"
      [ngClass]="[variantClass]"
      *ngIf="link; else buttonTpl"
      [routerLink]="link"
      >
      <i *ngIf="icon" class="fa" [ngClass]="icon" aria-hidden="true"></i>
      <span *ngIf="label">{{ label }}</span>
    </a>

    <ng-template #buttonTpl>
      <button class="btn" [ngClass]="[variantClass]">
        <i *ngIf="icon" class="fa" [ngClass]="icon" aria-hidden="true"></i>
        <span *ngIf="label">{{ label }}</span>
      </button>
    </ng-template>
  `,
  styleUrls: ['./button.css'],
  imports: [CommonModule, RouterModule]
})
export class ButtonComponent {
  @Input() label = '';
  @Input() icon = '';
  @Input() variant: 'primary' | 'secondary' = 'primary';
  @Input() link: string | any[] | null = null;

  get variantClass() {
    return this.variant === 'primary' ? 'btn-primary' : '';
  }
}
