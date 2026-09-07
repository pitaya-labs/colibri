import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { Breadcrumb } from '@shared-ui/page-structure/breadcrumb';

interface MetaField {
  label: string;
  value: string;
}

@Component({
  selector: 'app-consultation-detail',
  imports: [RouterLink, MatButton, PageContainer, PageHeader, PageContent, Breadcrumb],
  templateUrl: './detail.html',
  styleUrl: './detail.css',
})
export default class Detail {
  readonly patientName = 'Paciente A-0871';
  readonly patientInitials = 'A8';
  readonly demographics = 'Masculino · 51 años (14/03/1975) · Expediente 871';

  readonly consultationMeta: MetaField[] = [
    { label: 'Médico responsable', value: 'Dra. M. Rivera' },
    { label: 'Consultorio', value: 'Consultorio Norte' },
    { label: 'Fecha y hora', value: '26 ago 2026, 08:30 (GMT-6)' },
    { label: 'Motivo de consulta', value: 'Dolor lumbar, 5 días' },
  ];

  readonly noteMeta = 'Editable · guardado automático hace 12 s · autor Dra. M. Rivera';
  readonly notePreview =
    'Dolor lumbar mecánico de 5 días de evolución, sin irradiación ni datos de alarma. ' +
    'Exploración con contractura paravertebral derecha…';

  readonly recordSummary: MetaField[] = [
    { label: 'Alergias', value: 'Penicilina' },
    { label: 'Padecimientos crónicos', value: 'Hipertensión arterial' },
    { label: 'Última consulta', value: '02 jun 2026 · Dra. M. Rivera' },
    { label: 'Notas finalizadas', value: '7 documentos' },
  ];

  readonly closingNotice =
    'La consulta solo puede cerrarse con una nota finalizada o una causa documentada. ' +
    'La nota actual sigue en borrador.';
}
