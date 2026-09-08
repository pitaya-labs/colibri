import { Component, inject } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { Breadcrumb } from '@shared-ui/page-structure/breadcrumb';
import { ModalService } from '@shared-services/modal-service';

interface MetaField {
  label: string;
  value: string;
}

interface Vital {
  label: string;
  value: string;
  unit: string;
}

interface HistoryEntry {
  kind: 'adenda' | 'version';
  icon: string;
  circleClass: string;
  titulo: string;
  fecha: string;
  autor: string;
  texto: string;
  hash: string;
}

@Component({
  selector: 'app-consultation-clinic-note',
  imports: [MatButton, PageContainer, PageHeader, PageContent, Breadcrumb],
  templateUrl: './clinic-note.html',
  styleUrl: './clinic-note.css',
})
export default class ClinicNote {
  private readonly modalService = inject(ModalService);

  readonly patientName = 'Paciente A-0871';
  readonly demographics =
    'Masculino · 51 años · Expediente 871 · Consulta del 26 ago 2026, 08:30 · Consultorio Norte';

  readonly author = 'Dra. M. Rivera';
  readonly finalizedAt = '26 ago 2026, 09:42 (GMT-6)';
  readonly version = 'v1 · 2 adendas';
  readonly hashShort = 'a7f3…9c1d';
  private readonly hashFull = 'a7f3b1c9e5d24f80a1b6c3d7e9f012345678901234567890abcdef0123459c1d';

  readonly reference: MetaField[] = [
    { label: 'Motivo de consulta', value: 'Dolor lumbar, 5 días' },
    { label: 'Consulta asociada', value: '26 ago 2026, 08:30 (GMT-6)' },
    { label: 'Médico autor', value: 'Dra. M. Rivera' },
  ];

  readonly evolution =
    'Dolor lumbar mecánico de 5 días, sin irradiación ni datos de alarma neurológica. ' +
    'Niega fiebre, pérdida de peso y trauma previo. A la exploración: contractura ' +
    'paravertebral derecha, Lasègue negativo bilateral, marcha normal.';

  readonly vitals: Vital[] = [
    { label: 'Tensión arterial', value: '128/82', unit: 'mmHg' },
    { label: 'Frecuencia cardiaca', value: '76', unit: 'lpm' },
    { label: 'Frecuencia respiratoria', value: '16', unit: 'rpm' },
    { label: 'Temperatura', value: '36.4', unit: '°C' },
    { label: 'Peso', value: 'No capturado', unit: '' },
    { label: 'Talla', value: 'No capturada', unit: '' },
  ];

  readonly study = 'Radiografía lumbar — 12 jun 2026';

  readonly diagnoses = [
    'M54.5 — Lumbalgia no especificada',
    'I10 — Hipertensión esencial (comorbilidad conocida)',
  ];

  readonly prognosis = 'Bueno. Mejoría esperada en 7–10 días con manejo conservador.';

  readonly treatment =
    'Paracetamol 500 mg cada 8 h por 5 días. Calor local dos veces al día. Evitar cargas ' +
    'mayores a 5 kg. Cita de revaloración en 7 días o antes si aparece debilidad, fiebre o ' +
    'alteración de esfínteres.';

  readonly signature = 'Firmado y bloqueado por Dra. M. Rivera · 26 ago 2026, 09:42 (GMT-6)';

  private readonly adendaCircle = 'bg-secondary-container text-on-secondary-container';
  private readonly versionCircle =
    'bg-(--mat-app-doc-finalizado-container) text-(--mat-app-on-doc-finalizado-container)';

  readonly history: HistoryEntry[] = [
    {
      kind: 'adenda',
      icon: 'post_add',
      circleClass: this.adendaCircle,
      titulo: 'Adenda 2 — Resultado de laboratorio',
      fecha: '27 ago 2026, 11:20',
      autor: 'Dra. M. Rivera',
      texto:
        'Se incorpora resultado de biometría hemática solicitada en la consulta: sin ' +
        'alteraciones relevantes. Se mantiene el plan inicial.',
      hash: 'c2b8…4e07',
    },
    {
      kind: 'adenda',
      icon: 'post_add',
      circleClass: this.adendaCircle,
      titulo: 'Adenda 1 — Corrección de dosis indicada',
      fecha: '26 ago 2026, 18:05',
      autor: 'Dra. M. Rivera',
      texto:
        'Se aclara que la indicación de paracetamol es de 500 mg cada 8 horas, según lo ' +
        'explicado al paciente durante la consulta.',
      hash: 'e91a…77bc',
    },
    {
      kind: 'version',
      icon: 'lock',
      circleClass: this.versionCircle,
      titulo: 'Versión 1 — Nota finalizada',
      fecha: '26 ago 2026, 09:42',
      autor: 'Dra. M. Rivera',
      texto: 'Documento original bloqueado. Contenido inmutable desde esta fecha.',
      hash: 'a7f3…9c1d',
    },
  ];

  printNote(): void {
    window.print();
  }

  async copyHash(): Promise<void> {
    try {
      await navigator.clipboard?.writeText(this.hashFull);
    } catch {
      // Clipboard no disponible; se informa de todas formas.
    }
    this.modalService.showConfirmationModal(
      'Hash copiado',
      'El hash SHA-256 completo del documento se copió al portapapeles.',
    );
  }

  verifyIntegrity(): void {
    this.modalService.showConfirmationModal(
      'Integridad verificada',
      'El hash SHA-256 coincide con el contenido almacenado. El documento no ha sido alterado.',
    );
  }
}
