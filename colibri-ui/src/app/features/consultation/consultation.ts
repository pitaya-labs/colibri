import { Component, computed, ElementRef, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatFormField, MatInput, MatLabel, MatSuffix } from '@angular/material/input';
import { PageContainer } from '@shared-ui/page-structure/page-container';
import { PageHeader } from '@shared-ui/page-structure/page-header';
import { PageContent } from '@shared-ui/page-structure/page-content';
import { ModalService } from '@shared-services/modal-service';
import { MatExpansionPanel, MatExpansionPanelHeader } from '@angular/material/expansion';
import { MatChipInputEvent } from '@angular/material/chips';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatOption, MatRipple } from '@angular/material/core';
import {
  MatAutocomplete,
  MatAutocompleteSelectedEvent,
  MatAutocompleteTrigger,
} from '@angular/material/autocomplete';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonToggle, MatButtonToggleGroup } from '@angular/material/button-toggle';

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

export interface Cie10Entry {
  code: string;
  description: string;
  searchText: string;
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
    MatSuffix,
    MatExpansionPanel,
    MatExpansionPanelHeader,
    MatRipple,
    MatAutocompleteTrigger,
    MatAutocomplete,
    MatOption,
    ReactiveFormsModule,
    MatButtonToggleGroup,
    MatButtonToggle,
  ],
  templateUrl: './consultation.html',
  styleUrl: './consultation.css',
})
export default class Consultation {
  private readonly modalService = inject(ModalService);

  readonly savedSecondsAgo = 12;

  readonly patient = 'Paciente A-0871 · M 51 a';
  readonly reason = 'Dolor lumbar, 5 días';
  readonly associatedConsultation = '26 ago 2026, 08:30';
  readonly authorDoctor = 'Dra. M. Rivera';
  readonly createdAt = '26 ago, 08:34';

  searchControl = new FormControl<string | Cie10Entry>('');
  private searchTerm = toSignal(this.searchControl.valueChanges, { initialValue: '' });

  readonly cie10Catalog: Array<{ code: string; description: string }> = [
    { code: 'A00.9', description: 'Cólera, no especificado' },
    { code: 'A09.0', description: 'Otras gastroenteritis y colitis de origen infeccioso' },
    { code: 'A09.9', description: 'Gastroenteritis y colitis de origen no especificado' },
    { code: 'A15.0', description: 'Tuberculosis del pulmón, confirmada por frotis del esputo' },
    { code: 'A36.9', description: 'Difteria, no especificada' },
    { code: 'A37.9', description: 'Tos ferina, no especificada' },
    { code: 'A41.9', description: 'Sepsis, no especificada' },
    { code: 'A49.9', description: 'Infección bacteriana, no especificada' },
    { code: 'A90', description: 'Fiebre del dengue [dengue clásico]' },
    { code: 'B01.9', description: 'Varicela sin complicaciones' },
    { code: 'B02.9', description: 'Herpes zóster sin complicaciones' },
    { code: 'B05.9', description: 'Sarampión sin complicaciones' },
    {
      code: 'B20',
      description: 'Enfermedad por VIH, resultante en enfermedades infecciosas y parasitarias',
    },
    {
      code: 'B24',
      description:
        'Enfermedad por virus de la inmunodeficiencia humana [VIH], sin otra especificación',
    },
    { code: 'B34.9', description: 'Infección viral, no especificada' },
    { code: 'B37.0', description: 'Estomatitis candidiásica' },
    { code: 'B82.9', description: 'Parasitosis intestinal, sin otra especificación' },
    { code: 'C16.9', description: 'Tumor maligno del estómago, parte no especificada' },
    { code: 'C18.9', description: 'Tumor maligno del colon, parte no especificada' },
    {
      code: 'C34.9',
      description: 'Tumor maligno de los bronquios o del pulmón, parte no especificada',
    },
    { code: 'C50.9', description: 'Tumor maligno de la mama, parte no especificada' },
    { code: 'C53.9', description: 'Tumor maligno del cuello del útero, sin otra especificación' },
    { code: 'C61', description: 'Tumor maligno de la próstata' },
    { code: 'D50.9', description: 'Anemia por deficiencia de hierro sin especificación' },
    { code: 'D64.9', description: 'Anemia de tipo no especificado' },
    { code: 'E03.9', description: 'Hipotiroidismo, no especificado' },
    { code: 'E05.9', description: 'Tirotoxicosis con o sin bocio, no especificada' },
    {
      code: 'E10.9',
      description: 'Diabetes mellitus insulinodependiente, sin mención de complicación',
    },
    {
      code: 'E11.2',
      description: 'Diabetes mellitus no insulinodependiente con complicaciones renales',
    },
    {
      code: 'E11.9',
      description: 'Diabetes mellitus no insulinodependiente, sin mención de complicación',
    },
    { code: 'E66.0', description: 'Obesidad debida a exceso de calorías' },
    { code: 'E66.9', description: 'Obesidad, no especificada' },
    { code: 'E78.0', description: 'Hipercolesterolemia pura' },
    { code: 'E78.2', description: 'Hiperlipidemia mixta' },
    { code: 'E86', description: 'Depleción del volumen (Deshidratación)' },
    {
      code: 'F10.2',
      description:
        'Trastornos mentales y del comportamiento debidos al uso de alcohol: síndrome de dependencia',
    },
    { code: 'F32.9', description: 'Episodio depresivo, no especificado' },
    { code: 'F41.1', description: 'Trastorno de ansiedad generalizada' },
    { code: 'F41.9', description: 'Trastorno de ansiedad, no especificado' },
    { code: 'F43.0', description: 'Reacción al estrés agudo' },
    { code: 'G40.9', description: 'Epilepsia, tipo no especificado' },
    { code: 'G43.9', description: 'Migraña, no especificada' },
    { code: 'G44.2', description: 'Cefalea debida a tensión' },
    {
      code: 'G47.0',
      description: 'Trastornos del inicio y del mantenimiento del sueño [insomnios]',
    },
    { code: 'H10.9', description: 'Conjuntivitis, no especificada' },
    { code: 'H52.1', description: 'Miopía' },
    { code: 'H52.4', description: 'Presbicia' },
    { code: 'H66.9', description: 'Otitis media, no especificada' },
    { code: 'I10', description: 'Hipertensión esencial (primaria)' },
    { code: 'I20.9', description: 'Angina de pecho, no especificada' },
    { code: 'I21.9', description: 'Infarto agudo del miocardio, sin otra especificación' },
    { code: 'I25.1', description: 'Enfermedad aterosclerótica del corazón' },
    { code: 'I50.9', description: 'Insuficiencia cardíaca, no especificada' },
    {
      code: 'I64',
      description:
        'Accidente vascular encefálico agudo, no especificado como hemorrágico o isquémico',
    },
    { code: 'I84.9', description: 'Hemorroides no especificadas, sin complicación' },
    { code: 'J00', description: 'Rinofaringitis aguda [resfriado común]' },
    { code: 'J02.9', description: 'Faringitis aguda, no especificada' },
    { code: 'J03.9', description: 'Amigdalitis aguda, no especificada' },
    { code: 'J04.0', description: 'Laringitis aguda' },
    {
      code: 'J06.9',
      description: 'Infección aguda de las vías respiratorias superiores, no especificada',
    },
    { code: 'J18.9', description: 'Neumonía, no especificada' },
    { code: 'J20.9', description: 'Bronquitis aguda, no especificada' },
    { code: 'J30.4', description: 'Rinitis alérgica, no especificada' },
    { code: 'J44.9', description: 'Enfermedad pulmonar obstructiva crónica, no especificada' },
    { code: 'J45.9', description: 'Asma, no especificada' },
    { code: 'K21.9', description: 'Enfermedad del reflujo gastroesofágico sin esofagitis' },
    { code: 'K29.7', description: 'Gastritis, no especificada' },
    { code: 'K30', description: 'Dispepsia' },
    { code: 'K35.8', description: 'Otras apendicitis agudas y las no especificadas' },
    {
      code: 'K40.9',
      description: 'Hernia inguinal unilateral o no especificada, sin obstrucción ni gangrena',
    },
    { code: 'K58.9', description: 'Síndrome del colon irritable sin diarrea' },
    { code: 'K59.0', description: 'Constipación' },
    { code: 'K80.2', description: 'Cálculo de la vesícula biliar sin colecistitis' },
    { code: 'L03.9', description: 'Celulitis de sitio no especificado' },
    { code: 'L20.9', description: 'Dermatitis atópica, no especificada' },
    { code: 'L29.9', description: 'Prurito, no especificado' },
    { code: 'L70.0', description: 'Acné vulgar' },
    { code: 'M16.9', description: 'Coxartrosis, no especificada' },
    { code: 'M17.9', description: 'Gonartrosis, no especificada' },
    { code: 'M54.2', description: 'Cervicalgia' },
    { code: 'M54.5', description: 'Lumbago no especificado' },
    { code: 'M79.1', description: 'Mialgia' },
    { code: 'M79.7', description: 'Fibromialgia' },
    { code: 'N18.9', description: 'Enfermedad renal crónica, no especificada' },
    { code: 'N20.1', description: 'Cálculo del uréter' },
    { code: 'N39.0', description: 'Infección de vías urinarias, sitio no especificado' },
    { code: 'N40', description: 'Hiperplasia de la próstata' },
    { code: 'O80.0', description: 'Parto único espontáneo, presentación cefálica de vértice' },
    { code: 'R05', description: 'Tos' },
    { code: 'R07.4', description: 'Dolor en el pecho, no especificado' },
    { code: 'R10.4', description: 'Otros dolores abdominales y los no especificados' },
    { code: 'R11', description: 'Náusea y vómito' },
    { code: 'R42', description: 'Mareo y desvanecimiento' },
    { code: 'R50.9', description: 'Fiebre, no especificada' },
    { code: 'R51', description: 'Cefalea' },
    { code: 'R53', description: 'Malestar y fatiga' },
    { code: 'S09.9', description: 'Traumatismo de la cabeza, no especificado' },
    {
      code: 'S60.9',
      description: 'Traumatismo superficial de la muñeca y de la mano, no especificado',
    },
    { code: 'S93.4', description: 'Esguinces y torceduras del tobillo' },
    { code: 'T14.9', description: 'Traumatismo, no especificado' },
  ];

  CIE10_CATALOG: Cie10Entry[] = this.cie10Catalog.map((item) => ({
    ...item,
    searchText: `${this.normalize(item.code)} ${this.normalize(item.description)}`,
  }));

  readonly diagnoses = signal<string[]>([]);
  readonly linkedStudies = signal<string[]>(['Radiografía lumbar · 12 jun 2026']);

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

  filteredCatalog = computed(() => {
    const raw = this.searchTerm();
    const query =
      typeof raw === 'string'
        ? raw
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .trim()
        : '';

    if (!query) {
      return this.CIE10_CATALOG.slice(0, 20);
    }

    return this.CIE10_CATALOG.filter((item) => item.searchText.includes(query)).slice(0, 20);
  });

  normalize(val: string): string {
    return val
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '');
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
        'Si, finalizar y bloquear',
      )
      .subscribe((confirmed) => console.log('finalizar nota', confirmed));
  }

  displayFn(item: Cie10Entry | null): string {
    return item ? `${item.code} - ${item.description}` : '';
  }

  onSelected(event: MatAutocompleteSelectedEvent): void {
    const selected: Cie10Entry = event.option.value;
    const diagnosis = `${selected.code} ${selected.description}`;
    this.diagnoses.update((list) => [...list, diagnosis]);
    this.searchControl.reset();
  }
}
