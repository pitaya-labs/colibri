import { Component } from '@angular/core';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { MatButton } from '@angular/material/button';
import { MatFormField, MatInput, MatLabel, MatPrefix } from '@angular/material/input';
import { MatOption } from '@angular/material/core';
import { MatSelect } from '@angular/material/select';
import { DatePipe } from '@angular/common';
import { MatCell, MatCellDef, MatColumnDef, MatHeaderCell,
  MatHeaderCellDef, MatHeaderRow, MatHeaderRowDef, MatRow, MatRowDef, MatTable } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { RouterLink } from '@angular/router';

@Component({
  imports: [
    PageContent,
    PageHeader,
    PageContainer,
    MatButton,
    MatFormField,
    MatLabel,
    MatInput,
    MatPrefix,
    MatOption,
    MatSelect,
    DatePipe,
    MatCell,
    MatCellDef,
    MatColumnDef,
    MatHeaderCell,
    MatHeaderRow,
    MatHeaderRowDef,
    MatPaginator,
    MatRow,
    MatRowDef,
    MatTable,
    MatHeaderCellDef,
    RouterLink,
  ],
  selector: 'app-list',
  styleUrl: './list.css',
  templateUrl: './list.html',
})
export class List {
  data = [
    {
      id: 1,
      name: 'Adriana Perez',
      birthDate: new Date(),
      telephone: '8127712285',
      lastConsultant: new Date(),
      actions: '',
    },
    {
      id: 2,
      name: 'Jorge Gutierrez',
      birthDate: new Date(),
      telephone: '6121197343',
      lastConsultant: new Date(),
      actions: '',
    },
  ];

  displayedColumns: string[] = ['name', 'birthDate', 'telephone', 'lastConsultant', 'actions'];
}
