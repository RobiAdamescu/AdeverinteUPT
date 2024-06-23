export class StudentResponse {
    id!: number;
    responses!: { [key: string]: string };
    reason!: string;
    filePath!: string;
    responseDate!: Date;
    status!: string;
    templateID!: number;
    studentID!: number;
    templateName!: string;
  }