export type PageModalType = 'Error' | 'Confirmation' | 'YesNo';

export interface PageModalModel {
  type: PageModalType;
  title: string;
  body: string;
}

