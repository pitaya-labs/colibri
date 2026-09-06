import { Injectable, inject } from '@angular/core';
import { ConsultationService } from './consultation-service';

@Injectable()
export class ConsultationFacade {
  protected readonly service = inject(ConsultationService);
}
