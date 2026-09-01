import { Component } from '@angular/core';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { MatButton } from '@angular/material/button';
import { MatCell, MatCellDef, MatColumnDef, MatHeaderCell, MatHeaderCellDef, MatHeaderRow,
  MatHeaderRowDef, MatRow, MatRowDef, MatTable } from '@angular/material/table';

@Component({
  imports: [
    PageContainer,
    PageHeader,
    PageContent,
    MatButton,
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
      name: 'item 1',
    },
  ];

  displayedColumns: string[] = ['id', 'name'];
}
