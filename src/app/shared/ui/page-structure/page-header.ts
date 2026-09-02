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

      .info {
        font-size: 14px;
        color: var(--color-gray-400);
        margin: 6px 0 0 0;
      }
    }
  `,
  template: `
    <header class="page-header">
      <div class="title">
        <ng-content select="[pageHeaderTitle]"></ng-content>
        <div class="info">
          <ng-content select="[pageHeaderInformation]"> </ng-content>
        </div>
      </div>
      <ng-content select="[pageHeaderActions]"></ng-content>
    </header>
  `,
})
export class PageHeader {}
