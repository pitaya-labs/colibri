import { Component, computed, inject, signal } from '@angular/core';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { Breadcrumb } from '@shared-ui/page-structure/breadcrumb';
import { MatFormField, MatLabel, MatInput, MatPrefix } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatAutocomplete, MatAutocompleteSelectedEvent, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { MatButton } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';

interface Patient {
  id: number;
  nombre: string;
  sexo: string;
  edad: number;
  expediente: string;
  telefono: string;
}

@Component({
  imports: [
    PageContainer,
    PageHeader,
    PageContent,
    Breadcrumb,
    MatFormField,
    MatLabel,
    MatInput,
    MatPrefix,
    MatSelect,
    MatOption,
    MatAutocomplete,
    MatAutocompleteTrigger,
    MatButton,
    RouterLink,
  ],
  selector: 'app-create',
  styleUrl: './create.css',
  templateUrl: './create.html',
})
export class Create {
  private router = inject(Router);

  patients: Patient[] = [
    { id: 1, nombre: 'Paciente A-0871', sexo: 'M', edad: 51, expediente: '871', telefono: '55 1234 5678' },
    { id: 2, nombre: 'Paciente A-0812', sexo: 'F', edad: 39, expediente: '812', telefono: '55 8765 4321' },
    { id: 3, nombre: 'Paciente A-0899', sexo: 'M', edad: 24, expediente: '899', telefono: '55 2233 1100' },
  ];

  horariosLibres = ['09:00', '09:30', '10:30', '11:00', '12:00'];

  readonly newPatientOption = 'new' as const;

  searchQuery = signal('');
  selectedPatient = signal<Patient | null>(null);
  selectedHorario = signal<string | null>(null);

  filteredPatients = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();

    if (!query) {
      return [];
    }

    return this.patients.filter(
      (p) => p.nombre.toLowerCase().includes(query) || p.expediente.includes(query),
    );
  });

  onSearchInput(value: string) {
    this.searchQuery.set(value);
  }

  displayPatient = (patient: Patient) => patient?.nombre ?? '';

  onPatientSelected(event: MatAutocompleteSelectedEvent) {
    const value = event.option.value;

    if (value === this.newPatientOption) {
      this.router.navigateByUrl('/patients/create');
      return;
    }

    this.selectPatient(value as Patient);
  }

  selectPatient(patient: Patient) {
    this.selectedPatient.set(patient);
    this.searchQuery.set('');
  }

  clearPatient() {
    this.selectedPatient.set(null);
  }

  selectHorario(horario: string) {
    this.selectedHorario.set(horario);
  }
}
