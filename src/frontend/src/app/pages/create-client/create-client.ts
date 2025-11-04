import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-create-client',
  standalone: true,
  templateUrl: './create-client.html',
  imports: [CommonModule, PageTitleComponent]
})
export class CreateClientComponent {}
