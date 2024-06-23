import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StudentResponse } from '../models/student-response.model';

@Injectable({
  providedIn: 'root'
})
export class StudentResponseApiService {
  private baseUrl: string = "https://localhost:7208/api/StudentResponse";

  constructor(private http: HttpClient) { }

  createStudentResponse(data: any) {
    return this.http.post(`${this.baseUrl}`, data);
  }

  deleteStudentResponse(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
  viewCompletedCertificate(fileName: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/view/${fileName}`);
  }
  downloadCompletedCertificate(filePath: string) {
    const fileName = filePath.split('\\').pop() || ''; 
    const url = `${this.baseUrl}/download/${fileName}`;
    window.open(url, '_blank');
  }
  updateStudentResponse(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, data);
  }

  getStudentResponsesByStudentId(studentId: number): Observable<StudentResponse[]> {
    return this.http.get<StudentResponse[]>(`${this.baseUrl}/student/${studentId}`);
  }

  getAllStudentResponses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/all`);
  }

  getStudentDetails(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/students`);
  }

  acceptStudentResponse(id: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}/accept`, {});
  }
  refuseStudentResponse(id: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}/refuse`, {});
  }

  revertStudentResponse(id: number): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}/revert`, {});
  }


}
