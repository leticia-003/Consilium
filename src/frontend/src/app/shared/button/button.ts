import { Component, Input, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-button',
  standalone: true,
  templateUrl: './button.html',
  styleUrls: ['./button.css'],
  imports: [CommonModule, RouterModule],
})
export class ButtonComponent {
  @Input() label = '';
  @Input() icon = '';
  @Input() variant: 'primary' | 'secondary' = 'primary';
  @Input() link: string | any[] | null = null;
  @Input() disabled: boolean = false;

  @HostBinding('style.pointer-events')
  get pointerEvents(): string {
    return this.disabled ? 'none' : 'auto';
  }

  get variantClass() {
    return this.variant === 'primary' ? 'btn-primary' : '';
  }
}
