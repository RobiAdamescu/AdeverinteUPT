import { HttpInterceptorFn, HttpHeaders, HttpErrorResponse, HttpRequest, HttpHandler, HttpHandlerFn } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, switchMap, throwError } from 'rxjs';
import { TokenApiModel } from '../models/token-api.model';

export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const toastr = inject (ToastrService)
  const router = inject(Router)

  const myToken = auth.getToken();

  if(myToken){
    req = req.clone({
      setHeaders: {Authorization:`Bearer ${myToken}`}
    })
  }

  return next(req).pipe(
    catchError((err:any)=>{
      if(err instanceof HttpErrorResponse){
        if(err.status === 401){
          return handleUnAuthorizedError(req,next);
        }
      }
      return throwError(()=> new Error("Some other error occured"))
    })
  );
function handleUnAuthorizedError(req: HttpRequest<any>, next:HttpHandlerFn){
  let tokenApiModel = new TokenApiModel();
  tokenApiModel.accessToken = auth.getToken()!;
  tokenApiModel.refreshToken = auth.getRefreshToken()!;
  return auth.renewToken(tokenApiModel).pipe(
    switchMap((data:TokenApiModel)=>{
      auth.storeRefreshToken(data.refreshToken);
      auth.storeToken(data.accessToken);
      req = req.clone({
        setHeaders: {Authorization:`Bearer ${data.accessToken}`}
      })
      return next(req)
    }),
    catchError((err)=>{
      return throwError(()=>{
        toastr.warning("Warning","Token is expired, please login again");
        router.navigate(['login']);
      })
    })
  )
}
}