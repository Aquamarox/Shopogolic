CREATE TABLE IF NOT EXISTS "Accounts" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Balance" decimal(18,2) NOT NULL,
    "HeldAmount" decimal(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "Version" integer NOT NULL,
    CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Accounts_UserId" ON "Accounts" ("UserId");

CREATE TABLE IF NOT EXISTS "Transactions" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "Type" integer NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ProcessedAt" timestamp with time zone,
    CONSTRAINT "PK_Transactions" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Transactions_OrderId" ON "Transactions" ("OrderId");

CREATE TABLE IF NOT EXISTS "InboxMessages" (
    "Id" uuid NOT NULL,
    "MessageId" character varying(200) NOT NULL,
    "EventType" character varying(100) NOT NULL,
    "Payload" text NOT NULL,
    "IsProcessed" boolean NOT NULL,
    "ReceivedAt" timestamp with time zone NOT NULL,
    "ProcessedAt" timestamp with time zone,
    CONSTRAINT "PK_InboxMessages" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_InboxMessages_MessageId" ON "InboxMessages" ("MessageId");
CREATE INDEX IF NOT EXISTS "IX_InboxMessages_IsProcessed_ReceivedAt" ON "InboxMessages" ("IsProcessed", "ReceivedAt");

CREATE TABLE IF NOT EXISTS "OutboxMessages" (
    "Id" uuid NOT NULL,
    "EventType" character varying(100) NOT NULL,
    "Payload" text NOT NULL,
    "IsSent" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "SentAt" timestamp with time zone,
    CONSTRAINT "PK_OutboxMessages" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_OutboxMessages_IsSent_CreatedAt" ON "OutboxMessages" ("IsSent", "CreatedAt");
