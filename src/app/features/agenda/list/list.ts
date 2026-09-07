import { Component } from '@angular/core';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { MatButton } from '@angular/material/button';
import { MatCell, MatCellDef, MatColumnDef, MatHeaderCell, MatHeaderCellDef, MatHeaderRow,
  MatHeaderRowDef, MatRow, MatRowDef, MatTable } from '@angular/material/table';
import { MatFormField, MatInput, MatLabel, MatPrefix } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatButtonToggle, MatButtonToggleGroup } from '@angular/material/button-toggle';
import { DatePipe } from '@angular/common';
import { MatPaginator } from '@angular/material/paginator';
import { RouterLink } from '@angular/router';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  imports: [
    PageContainer,
    PageHeader,
    PageContent,
    MatButton,
    RouterLink,
    MatTable,
    MatColumnDef,
    MatHeaderCell,
    MatCell,
    MatHeaderCellDef,
    MatCellDef,
    MatHeaderRow,
    MatRow,
    MatRowDef,
    MatHeaderRowDef,
    MatFormField,
    MatLabel,
    MatSelect,
    MatOption,
    MatButtonToggleGroup,
    MatButtonToggle,
    MatInput,
    MatPrefix,
    DatePipe,
    MatPaginator,
    MatTooltip,
  ],
  selector: 'app-list',
  styleUrl: './list.css',
  templateUrl: './list.html',
})
export default class List {
  data = [
    {
      id: 1,
      time: new Date(),
      patient: 'Jose Perez',
      reason: 'Dolor estomacal',
      type: 'consulta',
      doctor: 'Ana',
      state: 'Abierta',
      actions: '',
    },
    {
      id: 1,
      time: new Date(),
      patient: 'Andrea Gonzales',
      reason: 'Consulta regular',
      type: 'consulta',
      doctor: 'Pedro Perez',
      state: 'Cerrada',
      actions: '',
    },
  ];

  displayedColumns: string[] = ['time', 'patient', 'reason', 'type', 'doctor', 'state', 'actions'];
}
