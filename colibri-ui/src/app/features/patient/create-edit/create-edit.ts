import { Component, inject } from '@angular/core';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { Breadcrumb } from '@shared-ui/page-structure/breadcrumb';
import { MatFormField, MatHint, MatInput, MatLabel, MatPrefix } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatButton } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { ModalService } from '@shared-services/modal-service';

@Component({
  imports: [
    PageContainer,
    PageHeader,
    PageContent,
    Breadcrumb,
    MatFormField,
    MatLabel,
    MatInput,
    MatSelect,
    MatOption,
    MatPrefix,
    MatHint,
    MatButton,
    RouterLink,
  ],
  selector: 'app-create-edit',
  styleUrl: './create-edit.css',
  templateUrl: './create-edit.html',
})
export class CreateEdit {
  private readonly modalService: ModalService = inject(ModalService);

  onSave(): void {
    this.modalService
      .showYesNoModal('Paciente Creado Correctamente', 'Queires iniciar la consulta en este momento?')
      .subscribe(result => console.log(result));
  }
}
