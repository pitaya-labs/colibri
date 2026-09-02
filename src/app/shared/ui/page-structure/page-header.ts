import { afterNextRender, Component, DestroyRef, ElementRef, inject, input, signal, viewChild } from '@angular/core';

@Component({
  imports: [],
  selector: 'page-header',
  styles: `
    .page-header {
      display: flex;
      justify-content: space-between;
      gap: 4px;
      background-color: var(--color-surface-container-low);
      padding: 0 var(--padding-12);

      .title {
        font-size: 26px;
        font-weight: 400;

        .info {
          font-size: 14px;
          color: var(--color-gray-400);
          margin: 6px 0 0 0;
        }
      }
    }

    .header-on-sticky {
      position: sticky;
      top: 0;
      z-index: 10;

      &.is-stuck {
        padding-top: 12px;
        box-shadow: 0 10px 6px -4px var(--color-gray-200);
      }
    }

    .sticky-sentinel {
      height: 1px;
    }
  `,
  template: `
    <div class="sticky-sentinel" #stickySentinel></div>
    <header class="page-header" [class.header-on-sticky]="isSticky()" [class.is-stuck]="isStuck()">
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
export class PageHeader {
  isSticky = input<boolean>(false);
  isStuck = signal(false);

  private stickySentinel = viewChild.required<ElementRef<HTMLElement>>('stickySentinel');
  private destroyRef = inject(DestroyRef);

  constructor() {
    afterNextRender(() => {
      if (!this.isSticky()) {
        return;
      }

      const observer = new IntersectionObserver(
        ([entry]) => this.isStuck.set(!entry.isIntersecting),
        { threshold: 0 },
      );

      observer.observe(this.stickySentinel().nativeElement);
      this.destroyRef.onDestroy(() => observer.disconnect());
    });
  }
}
