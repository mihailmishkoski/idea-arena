import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { AuthRoutingModule } from './auth-routing.module';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';

@NgModule({
  declarations: [ConfirmEmailComponent, LoginComponent, RegisterComponent],
  imports: [SharedModule, AuthRoutingModule],
})
export class AuthModule {}
