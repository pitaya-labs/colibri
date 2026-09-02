import { Component } from '@angular/core';

@Component({
  imports: [],
  selector: 'page-content',
  template: `
    <section class="page-content">
      <ng-content></ng-content>
    </section>
  `,
  styles: `
    .page-content {
      display: block;
      padding-top: 20px;
      margin: 0 12px;
    }
  `,
})
export class PageContent {}
