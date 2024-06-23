import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { Template } from '../models/template.model';

@Injectable({
  providedIn: 'root'
})
export class AdminAPIService {
  private baseUrl: string = "https://localhost:7208/api/Template";

  constructor(private http: HttpClient) { }

  getTemplates() {
    return this.http.get<any>(this.baseUrl);
  }

  createTemplate(formData: FormData): Observable<any> {
    const token = localStorage.getItem('authToken');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.post(`${this.baseUrl}/create`, formData, { headers });
  }

  downloadTemplate(filePath: string) {
    const fileName = filePath.split('\\').pop() || '';
    const url = `${this.baseUrl}/download/${fileName}`;
    window.open(url, '_blank');
  }

  viewTemplate(fileName: string): Observable<{ Content: string }> {
    return this.http.get<{ Content: string }>(`${this.baseUrl}/view/${fileName}`);
  }

  updateTemplate(id: number, formData: FormData): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, formData);
  }

  deleteTemplate(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }

  getTemplateById(id: number): Observable<Template> {
    return this.http.get<Template>(`${this.baseUrl}/${id}`);
  }
  
  getFieldsByTemplateId(templateId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${templateId}/fields`);
  }
}
