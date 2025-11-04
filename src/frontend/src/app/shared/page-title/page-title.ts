import { Component, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-title',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './page-title.html',
  styleUrls: ['./page-title.css']
})
export class PageTitleComponent {
  @Input() title!: string;
  @Input() subtitle?: string;
  @Input() actions?: TemplateRef<any> | null;
}
