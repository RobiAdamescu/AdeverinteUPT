import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import {JwtHelperService} from '@auth0/angular-jwt';
import { TokenApiModel } from '../models/token-api.model';
import { Observable } from 'rxjs/internal/Observable';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl: string = "https://localhost:7208/api/User/";
  private userPayload: any;
  constructor(private http: HttpClient, private router: Router) {
    this.userPayload = this.decodedToken();
   }

  private isLocalStorageAvailable = typeof localStorage !=='undefined';

  login(loginObj: any) {
    return this.http.post<any>(`${this.baseUrl}authenticate`, loginObj);
  }

  signout(){
    localStorage.clear();
    this.router.navigate(['login']);
  }

  storeToken(tokenValue: string) {
    if (this.isLocalStorageAvailable) {
      localStorage.setItem('token', tokenValue);
    }
  }
  storeRefreshToken(tokenValue: string){
    if(this.isLocalStorageAvailable){
      localStorage.setItem('refreshToken',tokenValue)
    }
  }

  getToken() {
    if (this.isLocalStorageAvailable) {
      return localStorage.getItem('token');
    }
    return null;
  }
  getRefreshToken() {
    if (this.isLocalStorageAvailable) {
      return localStorage.getItem('refreshToken');
    }
    return null;
  }

  isLoggedIn(): boolean {
    if (this.isLocalStorageAvailable) {
      const token = localStorage.getItem('token');
      if(token){
        return true;
      }
    }
    return false;
  }

  decodedToken(){
    const jwtHelper = new JwtHelperService();
    const token = this.getToken()!;

    console.log(jwtHelper.decodeToken(token))
    return jwtHelper.decodeToken(token);
  }

  getEmailFromToken(){
    if (this.userPayload)
      return this.userPayload.unique_name;
  }
  
  getRoleFromToken(){
    if (this.userPayload)
      return this.userPayload.role;
  }

  renewToken(tokenApi: TokenApiModel){
    return this.http.post<any>(`${this.baseUrl}refresh`, tokenApi)
  }

  getUserIdByEmail(email: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}getUserIdByEmail/${email}`);
  }

  getUserDetailsByEmail(email: string): Observable<any> {
    return this.http.get(`${this.baseUrl}details/${email}`)
  }
}
