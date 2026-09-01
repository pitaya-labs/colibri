import { Component } from '@angular/core';

@Component({
  imports: [],
  selector: 'page-header',
  styles: `
    .page-header {
      display: flex;
      justify-content: space-between;

      .title {
        font-size: 26px;
        font-weight: 400;
      }
    }
  `,
  template: `
    <header class="page-header">
      <div class="title">
        <ng-content select="[pageHeaderTitle]"></ng-content>
      </div>
      <ng-content select="[pageHeaderActions]"></ng-content>
    </header>
  `,
})
export class PageHeader {}
