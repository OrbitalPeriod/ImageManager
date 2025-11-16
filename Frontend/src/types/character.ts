export interface GetCharacterResponse {
  tagId: number;                      
  characterName?: string;      
  count: number;                    
}

export interface GetCharacterResponsePaginatedResponse {
  data?: GetCharacterResponse[];   // nullable array
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}