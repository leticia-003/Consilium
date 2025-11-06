import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-lawyers',
  standalone: true,
  templateUrl: './lawyers.html',
  styleUrls: ['./lawyers.css'],
  imports: [PageTitleComponent]
})
export class LawyersComponent {}
