import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserStoreService } from '../../services/user-store.service';
import { AdminAPIService } from '../../services/admin-api.service';
import { Template } from '../../models/template.model';
import { StudentResponseApiService } from '../../services/student-response-api.service';
import { StudentResponse } from '../../models/student-response.model';
import { NgxPaginationModule } from 'ngx-pagination';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxPaginationModule],
  providers: [AuthService, AdminAPIService, StudentResponseApiService],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  public templates: Template[] = [];
  public filteredTemplates: Template[] = [];
  public email: string = "";
  public role: string = "";
  public isAdmin: boolean = false;
  public specializations: string[] = [];
  public selectedSpecialization: string = "";
  page: number = 1;
  searchQuery: string = '';
  currentPage: number = 1;
  itemsPerPage: number = 8;
  totalPages: number = 1;
  pages: number[] = [];
  public adeverinte: StudentResponse[] = [];
  public isAddTemplateFormVisible: boolean = false;
  public isViewTemplateFormVisible: boolean = false;
  public isDropdownOpen = false;
  public newTemplate: any = {
    name: '',
    specializations: '',
    fields: [''],
    file: null
  };
  public currentTemplate: any = {};
  public isEditTemplateFormVisible: boolean = false;
  public editTemplate: any = {
    id: null,
    name: '',
    specializations: '',
    fields: [''],
    file: null
  };

  public isRequestFormVisible: boolean = false;
  public isFillCertificateFormVisible: boolean = false;
  public selectedTemplateId: number | null = null;
  public formData: any = {
    reason: '',
    fields: {},
    facultate: '',
    an: ''
  };
  public userId: number | null = null;
  isViewCompletedCertificateFormVisible: boolean = false;
  currentCompletedCertificateContent: string = '';
  isEditResponseFormVisible: boolean = false;
  editResponseData: any = {};
  currentEditingResponseId: number | null = null;
  searchText: string = '';

  filteredRequests: any[] = [];
  selectedFilter: string = 'In asteptare';
  requests: any[] = [];
  students: any[] = [];
  yearFilter: string = '';
  student_specializations: string[] = [];
  selectedStudentSpecialization: string = '';
  student_faculty: string[] = [];
  selectedStudentFaculty: string = '';



  constructor(private adminApi: AdminAPIService, private auth: AuthService, private userStore: UserStoreService, private studentResponseApi: StudentResponseApiService) { }

  ngOnInit() {
    this.adminApi.getTemplates()
      .subscribe(res => {
        this.templates = res;
        this.filteredTemplates = res;
        this.specializations = Array.from(new Set(res.flatMap((template: Template) => template.specializations)));
      });

    this.userStore.getRoleFromStore().subscribe(val => {
      const roleFromToken = this.auth.getRoleFromToken();
      this.role = val || roleFromToken;
      this.isAdmin = this.role === 'admin';

      this.userStore.getEmailFromStore().subscribe(val => {
        const emailFromToken = this.auth.getEmailFromToken();
        this.email = val || emailFromToken;

      });

      if (this.role === 'secretar') {
        this.fetchAllRequests();
        this.fetchStudentDetails();
      } else if (this.role === 'student') {
        if (this.email) {
          this.fetchUserIdByEmail(this.email);
        }
      }
    });
  }

  getKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  logout() {
    this.auth.signout();
  }

  // Parte admin
  filterTemplates() {
    if (this.selectedSpecialization) {
      this.filteredTemplates = this.templates.filter(template =>
        template.specializations.includes(this.selectedSpecialization));
    } else {
      this.filteredTemplates = this.templates;
    }
  }

  showAddTemplateForm() {
    this.isAddTemplateFormVisible = true;
  }

  hideAddTemplateForm() {
    this.isAddTemplateFormVisible = false;
  }

  onFileChange(event: any) {
    if (event.target.files.length > 0) {
      this.newTemplate.file = event.target.files[0];
    }
  }

  addField() {
    this.newTemplate.fields.push('');
  }

  removeField(index: number) {
    this.newTemplate.fields.splice(index, 1);
  }

  trackByIndex(index: number, obj: any): any {
    return index;
  }

  submitTemplate() {
    const formData = new FormData();
    formData.append('name', this.newTemplate.name);
    formData.append('specializations', this.newTemplate.specializations);
    const fieldsString = this.newTemplate.fields.join(',');
    formData.append('fields', fieldsString);
    formData.append('file', this.newTemplate.file);

    this.adminApi.createTemplate(formData).subscribe(response => {
      console.log('Template created successfully', response);
      this.isAddTemplateFormVisible = false;
      this.ngOnInit();
    }, error => {
      console.error('Error creating template', error);
    });
  }


  showViewTemplateForm(template: any) {
    this.currentTemplate = { ...template };
    const fileName = template.filePath.split('\\').pop();
    this.adminApi.viewTemplate(fileName).subscribe(
      (res: any) => {
        this.currentTemplate.fileContent = this.sanitizeHtml(res.content);
        this.isViewTemplateFormVisible = true;
      },
      error => {
        console.error('Error viewing template:', error);
      }
    );
  }

  sanitizeHtml(html: string): string {
    const div = document.createElement('div');
    div.innerHTML = html;
    return div.innerHTML;
  }

  hideViewTemplateForm() {
    this.isViewTemplateFormVisible = false;
  }

  downloadTemplate(event: Event, filePath: string) {
    event?.preventDefault();
    this.adminApi.downloadTemplate(filePath);
  }

  showEditTemplateForm(template: Template) {
    this.editTemplate = {
      id: template.templateID,
      name: template.name,
      specializations: template.specializations.join(', '),
      fields: [...template.fields],
      file: template.filePath
    };
    this.isEditTemplateFormVisible = true;
  }

  hideEditTemplateForm() {
    this.isEditTemplateFormVisible = false;
  }

  submitEditTemplate() {
    const formData = new FormData();
    formData.append('name', this.editTemplate.name);
    formData.append('specializations', this.editTemplate.specializations);
    const fieldsString = this.editTemplate.fields.join(',');
    formData.append('fields', fieldsString);

    if (this.editTemplate.file instanceof File) {
      formData.append('file', this.editTemplate.file);
    }

    if (this.editTemplate.id) {
      this.adminApi.updateTemplate(this.editTemplate.id, formData).subscribe(response => {
        this.isEditTemplateFormVisible = false;
        this.ngOnInit();
      }, error => {
        console.error('Error updating template', error);
      });
    } else {
      console.error('Error: Template ID is not set.');
    }
  }

  onEditFileChange(event: any) {
    if (event.target.files.length > 0) {
      this.editTemplate.file = event.target.files[0];
    }
  }

  addFieldEdit() {
    this.editTemplate.fields.push('');
  }

  removeFieldEdit(index: number) {
    this.editTemplate.fields.splice(index, 1);
  }

  deleteTemplate(templateID: number): void {
    this.adminApi.deleteTemplate(templateID).subscribe(response => {
      console.log('Template deleted successfully', response);
      this.ngOnInit();
    }, error => {
      console.error('Error deleting template', error);
    });
  }

  // Parte student
  fetchStudentResponses(): void {
    if (this.userId !== null) {
      this.studentResponseApi.getStudentResponsesByStudentId(this.userId).subscribe(
        responses => {
          this.adeverinte = responses.filter(response =>
            response.templateName.toLowerCase().includes(this.searchText.toLowerCase())
          );
          console.log('Student responses fetched successfully:', responses);
        },
        error => {
          console.error('Error fetching student responses:', error);
        }
      );
    }
  }

  onSearchTextChanged(): void {
    this.fetchStudentResponses();
  }

  cerereNouaAdeverinta() {
    this.isRequestFormVisible = true;
  }

  hideRequestForm() {
    this.isRequestFormVisible = false;
  }

  fetchUserIdByEmail(email: string): void {
    this.auth.getUserDetailsByEmail(email).subscribe(
      response => {
        this.userId = response.userID;
        this.formData.facultate = response.facultate;
        this.formData.an = response.an;
        this.fetchStudentResponses();
      },
      error => {
        console.error('Error fetching user ID:', error);
      }
    );
  }

  selectTemplate(templateId: number | null): void {
    if (templateId !== null) {
      console.log('Selected template ID:', templateId);
      this.selectedTemplateId = templateId;
      this.isRequestFormVisible = false;
      this.isFillCertificateFormVisible = true;

      this.adminApi.getFieldsByTemplateId(templateId).subscribe(response => {
        if (response && response.fields) {
          this.formData.fields = {};
          if (Array.isArray(response.fields)) {
            response.fields.forEach((field: string) => {
              this.formData.fields[field] = '';
            });
          }

          else {
            console.error('Fields is not an array:', response.fields);
          }
        } else {
          console.error('Invalid fields response:', response);
        }
      }, error => {
        console.error('Error fetching fields:', error);
      });
    }
  }

  hideFillCertificateForm() {
    this.isFillCertificateFormVisible = false;
  }

  submitCertificate(): void {
    if (this.selectedTemplateId !== null && this.userId !== null) {
      const data = {
        Responses: JSON.stringify(this.formData.fields),
        Reason: this.formData.reason,
        TemplateID: this.selectedTemplateId,
        StudentID: this.userId,
        Facultate: this.formData.facultate,
        An: this.formData.an
      };

      this.studentResponseApi.createStudentResponse(data).subscribe(response => {
        console.log('Certificate request submitted successfully', response);
        this.hideFillCertificateForm();
        this.fetchStudentResponses();
      }, error => {
        console.error('Error submitting certificate request', error);
      });
    }

    else {
      console.error('selectedTemplateId or userId is null', { selectedTemplateId: this.selectedTemplateId, userId: this.userId });
    }
  }

  getCurrentTemplateFields(): string[] {
    const selectedTemplateIdNumber = Number(this.selectedTemplateId);
    const selectedTemplate = this.templates.find(t => t.templateID === selectedTemplateIdNumber);
    return selectedTemplate ? selectedTemplate.fields : [];
  }

  deleteStudentResponse(id: number): void {
    this.studentResponseApi.deleteStudentResponse(id).subscribe(
      response => {
        console.log('Student response deleted successfully', response);
        this.fetchStudentResponses();
      },
      error => {
        console.error('Error deleting student response', error);
      }
    );
  }

  showViewCompletedCertificateForm(filePath: string): void {
    const fileName = filePath.split('\\').pop();
    if (fileName) {
      this.studentResponseApi.viewCompletedCertificate(fileName).subscribe(response => {
        this.currentCompletedCertificateContent = response.content;
        this.isViewCompletedCertificateFormVisible = true;
      }, error => {
        console.error('Error fetching completed certificate content:', error);
      });
    } else {
      console.error('Invalid file path:', filePath);
    }
  }

  hideViewCompletedCertificateForm(): void {
    this.isViewCompletedCertificateFormVisible = false;
    this.currentCompletedCertificateContent = '';
  }

  downloadAdeverinta(event: Event, filePath: string) {
    event?.preventDefault();
    this.studentResponseApi.downloadCompletedCertificate(filePath);
  }

  submitEditResponse(): void {
    if (this.currentEditingResponseId !== null) {
      const formData = new FormData();
      formData.append('Responses', JSON.stringify(this.editResponseData.responses));
      formData.append('Reason', this.editResponseData.reason);

      if (this.editResponseData.file) {
        formData.append('File', this.editResponseData.file);
      }

      this.studentResponseApi.updateStudentResponse(this.currentEditingResponseId, formData).subscribe(response => {
        console.log('Response updated successfully', response);
        this.hideEditResponseForm();
        this.fetchStudentResponses();
        this.fetchAllRequests();
      }

        , error => {
          console.error('Error updating response', error);
        });
    } else {
      console.error('Error: Response ID is not set.');
    }
  }

  showEditResponseForm(response: any): void {
    this.currentEditingResponseId = response.id;
    this.editResponseData = {
      ...response,
      responses: JSON.parse(response.responses)
    };
    this.isEditResponseFormVisible = true;
  }

  hideEditResponseForm(): void {
    this.isEditResponseFormVisible = false;
    this.editResponseData = {};
    this.currentEditingResponseId = null;
  }

  //parte secretar
  fetchAllRequests() {
    this.studentResponseApi.getAllStudentResponses().subscribe(
      (data: any[]) => {
        this.requests = data;
        this.fetchStudentDetails();
        this.extractFaculties();
        this.extractSpecializations();
        this.applyFilter();
      },
      error => {
        console.error('Error fetching student responses:', error);
      }
    );
  }

  fetchStudentDetails() {
    this.studentResponseApi.getStudentDetails().subscribe((data: any[]) => {
      this.students = data;
      this.mergeRequestAndStudentData();
      this.extractFaculties();
      this.extractSpecializations();
      this.applyFilter();
    });
  }

  mergeRequestAndStudentData() {
    this.requests.forEach(request => {
      const student = this.students.find(s => s.userID === request.studentID);
      if (student) {
        request.StudentName = student.studentName;
        request.StudentFaculty = student.studentFaculty;
        request.StudentSpecialization = student.studentSpecialization;
      } else {
        console.log(`No student found for request ${request.id} with StudentID ${request.studentID}`);
      }
    });
    this.applyFilter();
  }

  filterRequests(status: string) {
    this.selectedFilter = status;
    console.log('Selected filter:', status);
    this.page = 1;
    this.applyFilter();
  }

  extractFaculties() {
    const facultiesSet = new Set<string>();
    this.requests.forEach(request => {
      if (request.StudentFaculty) {
        facultiesSet.add(request.StudentFaculty);
      }
    });
    this.student_faculty = Array.from(facultiesSet);
  }

  extractSpecializations() {
    const specializationsSet = new Set<string>();
    this.requests.forEach(request => {
      if (request.StudentSpecialization) {
        specializationsSet.add(request.StudentSpecialization);
      }
    });
    this.student_specializations = Array.from(specializationsSet);
  }

  filterRequestsByName(name: string) {
    this.searchQuery = name;
    this.applyFilter();
  }
  filterByYear(year: string) {
    this.yearFilter = year;
    this.applyFilter();
  }
  filterBySpecialization(specialization: string) {
    this.selectedStudentSpecialization = specialization;
    this.applyFilter();
  }
  filterByFaculty(faculty: string) {
    this.selectedStudentFaculty = faculty;
    this.applyFilter();
  }

  applyFilter() {
    let filtered = this.requests;
    if (this.selectedFilter) {
      filtered = filtered.filter(request => request.status === this.selectedFilter);
    }
    if (this.searchQuery) {
      filtered = filtered.filter(request => request.StudentName.toLowerCase().includes(this.searchQuery.toLowerCase()));
    }
    if (this.yearFilter) {
      filtered = filtered.filter(request => request.an === this.yearFilter);
    }
    if (this.selectedStudentSpecialization) {
      filtered = filtered.filter(request => request.StudentSpecialization === this.selectedStudentSpecialization);
    }
    if (this.selectedStudentFaculty) {
      filtered = filtered.filter(request => request.StudentFaculty === this.selectedStudentFaculty);
    }
    this.totalPages = Math.ceil(filtered.length / this.itemsPerPage);
    this.updatePagination();
    this.filteredRequests = filtered.slice((this.currentPage - 1) * this.itemsPerPage, this.currentPage * this.itemsPerPage);
  }

  confirmAction(action: () => void, message: string): void {
    if (confirm(message)) {
      action();
    }
  }

  acceptRequest(id: number): void {
    this.confirmAction(() => {
      this.studentResponseApi.acceptStudentResponse(id).subscribe(response => {
        this.fetchAllRequests();
      }, error => {
        console.error('Error accepting request', error);
      });
    }, 'Ești sigur că dorești să accepți această adeverință?');
  }

  refuseRequest(id: number): void {
    this.confirmAction(() => {
      this.studentResponseApi.refuseStudentResponse(id).subscribe(response => {
        this.fetchAllRequests();
      }, error => {
        console.error('Error refusing request', error);
      });
    }, 'Ești sigur că dorești să refuzi această adeverință?');
  }

  revertRequest(id: number): void {
    this.confirmAction(() => {
      this.studentResponseApi.revertStudentResponse(id).subscribe(response => {
        this.fetchAllRequests();
      }, error => {
        console.error('Error reverting request', error);
      });
    }, 'Ești sigur că dorești să trimiți adeverința înapoi în așteptare?');
  }

  printRequest(filePath: string): void {
    const fileName = filePath.split('\\').pop();
    if (fileName) {
      this.studentResponseApi.viewCompletedCertificate(fileName).subscribe(response => {
        const printWindow = window.open('', '_blank', 'width=800,height=600');
        if (printWindow) {
          printWindow.document.open();
          printWindow.document.write(`
          <html>
            <head>
              <title>Print</title>
              <style>
                @media print {
                  body {
                    margin: 0;
                    padding: 0;
                    box-shadow: none;
                  }
                  @page {
                    margin: 0;
                  }
                  /* Hide header, footer, and page numbering */
                  header, footer, .page-number {
                    display: none;
                  }
                }
              </style>
            </head>
            <body>
              ${response.content}
            </body>
          </html>
        `);
          printWindow.document.close();
          printWindow.onload = () => {
            printWindow.print();
            printWindow.close();
          };
        } else {
          console.error('Failed to open print window');
        }
      }, error => {
        console.error('Error fetching completed certificate content:', error);
      });
    } else {
      console.error('Invalid file path:', filePath);
    }
  }

  updatePagination() {
    this.pages = Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  changePage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.applyFilter();
    }
  }
}
