import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { AuthService } from '@core';
import { HeaderComponent } from './layout/header/header.component';
import { SharedModule } from './shared/shared.module';

/**
 * Restores the signed-in user from the auth cookie before the app renders, so
 * guards and the header see the correct auth state on first paint.
 */
function initAuth(auth: AuthService): () => unknown {
  return () => auth.loadCurrentUser();
}

@NgModule({
  declarations: [AppComponent, HeaderComponent],
  imports: [BrowserModule, FormsModule, CoreModule, SharedModule, AppRoutingModule],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initAuth,
      deps: [AuthService],
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
