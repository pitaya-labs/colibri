import { inject, Service } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PageModalModel, PageModalType } from '@shared-ui/page-modal/page-modal-model';
import { PageModal } from '@shared-ui/page-modal/page-modal';
import { map, Observable } from 'rxjs';

@Service()
export class ModalService {
  private readonly dialog = inject(MatDialog);

  showErrorModal(title?: string, body?: string): void {
    const type: PageModalType = 'Error';
    if (!title) title = 'Error';
    if (!body) body = 'Error';

    this.showModal(title, body, type);
  }

  showConfirmationModal(title?: string, body?: string): void {
    const type: PageModalType = 'Confirmation';
    if (!title) title = 'Confirmacion';
    if (!body) body = 'Alerta de confirmacion';
    this.showModal(title, body, type);
  }

  showYesNoModal(title?: string, body?: string): Observable<boolean> {
    const type: PageModalType = 'YesNo';
    if (!title) title = 'Respuesta';
    if (!body) body = 'Seleccione una respuesta';

    return this.showModal(title, body, type);
  }

  private showModal(title: string, body: string, type: PageModalType): Observable<boolean> {
    const dialogRef = this.dialog.open<PageModal, PageModalModel, any>(PageModal, {
      hasBackdrop: true,
      disableClose: true,
      data: {
        type,
        title,
        body,
      },
    });
    return dialogRef.afterClosed().pipe(map(result => result === true));
  }
}
