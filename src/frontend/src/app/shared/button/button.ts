import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-button',
  standalone: true,
  templateUrl: './button.html',
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
