import { Component, input } from '@angular/core';

@Component({
  imports: [],
  selector: 'page-breadcrumb',
  styles: `
    section {
      display: flex;
      flex-direction: row;
      padding: var(--padding-12) var(--padding-12) 0 var(--padding-12);

      span {
        margin-right: 10px;
        font-size: 12px;
        color: var(--color-gray-400);
      }

      span:last-child {
        color: var(--color-on-surface);
      }
    }
  `,
  template: `
    <section>
      @for (item of path(); track $index) {
        <span>{{ item }}</span>
        @if ($index < path().length - 1) {
          <span>&gt;</span>
        }
      }
    </section>
  `,
})
export class Breadcrumb {
  path = input<Array<string>>([]);
}
