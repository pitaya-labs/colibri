import { Component } from '@angular/core';

@Component({
  imports: [],
  selector: 'page-container',
  template: `
    <main class="page-structure-card">
      <ng-content select="page-breadcrumb"></ng-content>
      <ng-content select="page-header"></ng-content>
      <ng-content select="page-content"></ng-content>
    </main>
  `,
  styles: `
    .page-structure-card {
      display: block;
      justify-content: space-between;
      align-items: center;
      margin: 24px 12px;
    }
  `,
})
export class PageContainer {}
