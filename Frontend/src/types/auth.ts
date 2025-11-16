import type { Publicity } from "./publicity";

export interface RegisterRequest{
    email: string;
    password: string;
}

export interface LoginRequest {
  email:    string;
  password: string;
}

export interface GetUserInfoResponse {
  id: string;
  userName: string;
  email: string;
  defaultPublicity: Publicity;   
}

