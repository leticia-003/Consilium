import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-profiles',
  standalone: true,
  templateUrl: './profiles.html',
  styleUrls: ['./profiles.css'],
  imports: [PageTitleComponent]
})
export class ProfilesComponent {}
