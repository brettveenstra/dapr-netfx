// Copyright (c) 2025 DaprNetFx Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

// Sample code: Suppress overly broad exception handling warnings
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Sample demonstrates error handling patterns", Scope = "module")]

// Sample code: Allow multiple types in one file for simplicity
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Sample data models grouped for clarity", Scope = "module")]
