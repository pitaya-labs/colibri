import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogClose } from '@angular/material/dialog';
import { ModalData } from '@shared-ui/page-error-modal/page-modal-data';

@Component({
  imports: [MatButton, MatDialogClose],
  selector: 'page-error-modal',
  styleUrl: './page-error-modal.css',
  templateUrl: './page-error-modal.html',
})
export class PageErrorModal {
  // readonly dialogRef = inject(MatDialogRef<PageErrorModal>);
  readonly data = inject<ModalData>(MAT_DIALOG_DATA);
}
