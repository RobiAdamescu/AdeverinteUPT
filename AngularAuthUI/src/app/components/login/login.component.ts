import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { UserStoreService } from '../../services/user-store.service';



@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
  ReactiveFormsModule,
  CommonModule,
],
  providers:[
    AuthService,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {

  type: string = "password";
  isText: boolean = false;
  eyeIcon: string = "fa-eye-slash";
  loginForm!: FormGroup;

  constructor(
    private  fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService,
    private userStore: UserStoreService
    ){ }

  ngOnInit():void{
    this.loginForm = this.fb.group({
      email: ['', Validators.required],
      password: ['', Validators.required]      
    })
  }

  hideShowPass(){
    this.isText = !this.isText;
    this.isText ? this.eyeIcon = "fa-eye" : this.eyeIcon = "fa-eye-slash";
    this.isText ? this.type = "text" : this.type = "password";
  }

  onLogin(){
    if(this.loginForm.valid){
      console.log(this.loginForm.value);
      this.auth.login(this.loginForm.value)
      .subscribe({
        next:(res)=>{ 
          console.log(res.message);
          this.loginForm.reset();
          this.auth.storeToken(res.accessToken);
          this.auth.storeRefreshToken(res.refreshToken);
          const tokenPayload = this.auth.decodedToken();
          this.userStore.setEmailForStore(tokenPayload.email);
          this.userStore.setRoleForStore(tokenPayload.role);
          this.toastr.success("SUCCESS",res.message);
          this.router.navigate(['dashboard']);
        },
        error:(err)=>{
          
          this.toastr.error("ERROR","Something went wrong");
          console.log(err);
        }
      })

    }else{
      this.validateAllFormFields(this.loginForm);
      alert("Your form is invalid!")
    }
  }

  private validateAllFormFields(formGroup:FormGroup){
    Object.keys(formGroup.controls).forEach(field=>{
      const control = formGroup.get(field);
      if(control instanceof FormControl){
        control.markAsDirty({onlySelf:true});
      }else if(control instanceof FormGroup){
        this.validateAllFormFields(control)
      }
    })
  }
}
