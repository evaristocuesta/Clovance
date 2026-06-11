import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Translation, TranslocoLoader, provideTransloco } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';

import { LanguageSelection } from './language-selection';

class TranslocoTestingLoader implements TranslocoLoader {
  getTranslation(): Observable<Translation> {
    return of({});
  }
}

describe('LanguageSelection', () => {
  let component: LanguageSelection;
  let fixture: ComponentFixture<LanguageSelection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LanguageSelection],
      providers: [
        provideTransloco({
          config: {
            availableLangs: ['en', 'es'],
            defaultLang: 'en',
            reRenderOnLangChange: true,
            prodMode: true,
          },
          loader: TranslocoTestingLoader,
        }),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LanguageSelection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
