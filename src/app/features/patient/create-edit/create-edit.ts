import { Component } from '@angular/core';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { Breadcrumb } from '@shared-ui/page-structure/breadcrumb';
import { MatFormField, MatHint, MatInput, MatLabel, MatPrefix } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatButton } from '@angular/material/button';

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
  ],
  selector: 'app-create-edit',
  styleUrl: './create-edit.css',
  templateUrl: './create-edit.html',
})
export class CreateEdit {}
