import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatFormField, MatInput, MatLabel } from '@angular/material/input';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { Breadcrumb } from '@shared-ui/page-structure/breadcrumb';
import { ModalService } from '@shared-services/modal-service';

type Prognosis = 'bueno' | 'reservado' | 'malo';

interface PrognosisOption {
  value: Prognosis;
  label: string;
  icon: string;
}

interface Vital {
  label: string;
  value: string;
  unit: string;
}

@Component({
  selector: 'app-consultation',
  imports: [
    RouterLink,
    MatButton,
    MatFormField,
    MatLabel,
    MatInput,
    PageContainer,
    PageHeader,
    PageContent,
    Breadcrumb,
  ],
  templateUrl: './consultation.html',
  styleUrl: './consultation.css',
})
export default class Consultation {
  private readonly modalService = inject(ModalService);

  readonly savedSecondsAgo = 12;

  readonly patient = 'Paciente A-0871 · M 51 a';
  readonly reason = 'Dolor lumbar, 5 días';
  readonly associatedConsultation = '26 ago 2026, 08:30 (GMT-6)';
  readonly authorDoctor = 'Dra. M. Rivera';
  readonly createdAt = '26 ago, 08:34';

  readonly prognosisOptions: PrognosisOption[] = [
    { value: 'bueno', label: 'Bueno', icon: 'trending_up' },
    { value: 'reservado', label: 'Reservado', icon: 'trending_flat' },
    { value: 'malo', label: 'Malo', icon: 'trending_down' },
  ];

  readonly vitals: Vital[] = [
    { label: 'Tensión arterial', value: '128/82', unit: 'mmHg' },
    { label: 'Frecuencia cardiaca', value: '76', unit: 'lpm' },
    { label: 'Frecuencia respiratoria', value: '16', unit: 'rpm' },
    { label: 'Temperatura', value: '36.4', unit: '°C' },
    { label: 'Peso', value: '—', unit: 'kg' },
    { label: 'Talla', value: '—', unit: 'cm' },
  ];

  readonly vitalsSummary = 'TA 128/82 · FC 76 · T 36.4 °C';

  readonly diagnoses = signal<string[]>([
    'M54.5 Lumbalgia no especificada',
    'I10 Hipertensión esencial',
  ]);
  readonly linkedStudies = signal<string[]>(['Radiografía lumbar · 12 jun 2026']);

  readonly vitalsOpen = signal(true);
  readonly studiesOpen = signal(false);
  readonly prognosis = signal<Prognosis>('bueno');

  readonly evolution = signal(
    'Dolor lumbar mecánico de 5 días, sin irradiación ni datos de alarma neurológica. ' +
      'Niega fiebre, pérdida de peso y trauma previo. A la exploración: contractura ' +
      'paravertebral derecha, Lasègue negativo bilateral, marcha normal.',
  );
  readonly prognosisNote = signal('Mejoría esperada en 7–10 días con manejo conservador.');
  readonly treatment = signal(
    'Paracetamol 500 mg cada 8 h por 5 días. Calor local dos veces al día. Evitar cargas ' +
      'mayores a 5 kg. Cita de revaloración en 7 días o antes si aparece debilidad, fiebre o ' +
      'alteración de esfínteres.',
  );

  readonly evolutionLength = computed(() => this.evolution().length);

  toggleVitals(): void {
    this.vitalsOpen.update((open) => !open);
  }

  toggleStudies(): void {
    this.studiesOpen.update((open) => !open);
  }

  setPrognosis(value: Prognosis): void {
    this.prognosis.set(value);
  }

  removeDiagnosis(dx: string): void {
    this.diagnoses.update((list) => list.filter((item) => item !== dx));
  }

  removeStudy(study: string): void {
    this.linkedStudies.update((list) => list.filter((item) => item !== study));
  }

  onEvolutionInput(value: string): void {
    this.evolution.set(value);
  }

  onPrognosisNoteInput(value: string): void {
    this.prognosisNote.set(value);
  }

  onTreatmentInput(value: string): void {
    this.treatment.set(value);
  }

  saveDraft(): void {
    this.modalService.showConfirmationModal(
      'Borrador guardado',
      'La nota clínica se guardó como borrador. Sigue siendo visible solo para el autor hasta finalizar.',
    );
  }

  finalize(): void {
    this.modalService
      .showYesNoModal(
        '¿Finalizar esta nota clínica?',
        'La acción es irreversible: el documento quedará bloqueado de forma permanente y no ' +
          'podrá editarse ni eliminarse por ninguna vía. Las correcciones posteriores se ' +
          'registran como una adenda vinculada. ¿Deseas finalizar y bloquear la nota?',
        'Si, finalizar y bloquear'
      )
      .subscribe((confirmed) => console.log('finalizar nota', confirmed));
  }
}
