import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
  imports: [PageTitleComponent]
})
export class HomeComponent {}
