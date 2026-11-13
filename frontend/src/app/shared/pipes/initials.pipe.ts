import { Pipe, PipeTransform } from '@angular/core';

import { initialsOf } from '../../core/util/initials';

@Pipe({ name: 'initials' })
export class InitialsPipe implements PipeTransform {
  transform(fullName: string | null | undefined): string {
    return initialsOf(fullName);
  }
}
