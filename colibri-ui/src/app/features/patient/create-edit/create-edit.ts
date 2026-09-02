import { Component } from '@angular/core';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { Breadcrumb } from '@shared-ui/page-structure/breadcrumb';

@Component({
  imports: [PageContainer, PageHeader, PageContent, Breadcrumb],
  selector: 'app-create-edit',
  styleUrl: './create-edit.css',
  templateUrl: './create-edit.html',
})
export class CreateEdit {}
