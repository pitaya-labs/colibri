import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { appConfig } from './app.config';

describe('appConfig', () => {
  it('enables view transitions on route navigation', async () => {
    const originalStartViewTransition = (document as any).startViewTransition;
    const startViewTransitionSpy = vi.fn((callback: () => void) => {
      callback();
      return {
        finished: Promise.resolve(),
        ready: Promise.resolve(),
        updateCallbackDone: Promise.resolve(),
      };
    });
    (document as any).startViewTransition = startViewTransitionSpy;

    try {
      TestBed.configureTestingModule({
        providers: appConfig.providers,
      });

      const router = TestBed.inject(Router);
      await router.navigateByUrl('/agenda');
      await router.navigateByUrl('/patients');

      expect(startViewTransitionSpy).toHaveBeenCalled();
    } finally {
      (document as any).startViewTransition = originalStartViewTransition;
    }
  });
});
