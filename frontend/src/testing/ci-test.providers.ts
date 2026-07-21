import { provideHttpClient } from '@angular/common/http';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { EnvironmentProviders, Provider } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideTransloco, Translation, TranslocoLoader } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';

class TranslocoTestingLoader implements TranslocoLoader {
  getTranslation(): Observable<Translation> {
    return of({});
  }
}

const providers: Array<Provider | EnvironmentProviders> = [
  provideHttpClient(),
  provideRouter([]),
  provideTransloco({
    config: {
      availableLangs: ['en', 'es'],
      defaultLang: 'en',
      reRenderOnLangChange: true,
      prodMode: true,
    },
    loader: TranslocoTestingLoader,
  }),
  {
    provide: DialogRef,
    useValue: {
      close: () => {
        // No-op DialogRef used for unit tests that instantiate dialog components directly.
      },
    },
  },
  {
    provide: DIALOG_DATA,
    useValue: {},
  },
];

export default providers;