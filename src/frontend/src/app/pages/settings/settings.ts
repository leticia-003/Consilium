import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-settings',
  standalone: true,
  templateUrl: './settings.html',
  styleUrls: ['./settings.css'],
  imports: [PageTitleComponent]
})
export class SettingsComponent {}
