import { HttpErrorResponse } from '@angular/common/http';

import { toGeorgian } from './api-messages.ka';

/** A failed CMS request, ready to show: a headline plus per-field messages from validation. */
export interface ProblemView {
  readonly status: number;
  readonly title: string;
  readonly detail: string | null;
  readonly fields: readonly { readonly field: string; readonly messages: readonly string[] }[];
}

const titles: Readonly<Record<number, string>> = {
  0: 'სერვერთან კავშირი ვერ დამყარდა. შეამოწმეთ ინტერნეტი და სცადეთ თავიდან.',
  400: 'ზოგიერთი ველი არასწორადაა შევსებული.',
  401: 'სესია ამოიწურა. შედით თავიდან.',
  403: 'ამ მოქმედებისთვის უფლება არ გაქვთ.',
  404: 'ჩანაწერი ვერ მოიძებნა.',
  409: 'მოქმედება ვერ შესრულდა მიმდინარე მდგომარეობაში.',
  413: 'ფაილი ძალიან დიდია.',
  429: 'ძალიან ბევრი მცდელობა. მოიცადეთ ერთი წუთი.',
};

export function toProblem(error: unknown): ProblemView {
  if (!(error instanceof HttpErrorResponse)) {
    return { status: -1, title: 'მოულოდნელი შეცდომა.', detail: error instanceof Error ? error.message : null, fields: [] };
  }

  const body = (error.error ?? {}) as { detail?: string; errors?: Record<string, string[]> };
  const fields = Object.entries(body.errors ?? {}).map(([field, messages]) => ({
    field: field || '—',
    messages: messages.map(toGeorgian),
  }));
  let detail = typeof body.detail === 'string' ? body.detail : null;
  if (detail === 'One or more fields are invalid.') {
    detail = null;
  }
  if (error.status === 409 && detail?.toLowerCase().includes('changed by someone else')) {
    return {
      status: 409,
      title: 'ჩანაწერი სხვამ შეცვალა თქვენი გახსნის შემდეგ. გადატვირთეთ გვერდი და შეიტანეთ ცვლილებები თავიდან.',
      detail: null,
      fields,
    };
  }

  return {
    status: error.status,
    title: titles[error.status] ?? (error.status >= 500 ? 'სერვერის შეცდომა. სცადეთ მოგვიანებით.' : 'მოთხოვნა ვერ შესრულდა.'),
    detail: detail ? toGeorgian(detail) : null,
    fields,
  };
}
